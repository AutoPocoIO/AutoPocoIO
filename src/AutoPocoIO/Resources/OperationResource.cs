using AutoPocoIO.Context;
using AutoPocoIO.DynamicSchema.Db;
using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.DynamicSchema.Runtime;
using AutoPocoIO.DynamicSchema.Util;
using AutoPocoIO.Exceptions;
using AutoPocoIO.Extensions;
using AutoPocoIO.Factories;
using AutoPocoIO.Models;
using AutoPocoIO.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.AutoPoco;
using System.Linq.Dynamic.Core;

namespace AutoPocoIO.Resources
{
    public abstract class OperationResource : IDynamicResource
    {
        private readonly AppDbContext AppDbContext;
        private readonly IConnectionStringFactory _connctionStringFactory;

        public virtual Connector Connector { get; protected set; }
        public string DatabaseName => Connector.InitialCatalog;
        public string SchemaName => Connector.Schema;
        public virtual string DbObjectName { get; private set; }
        public virtual IDbSchema DbSchema { get; }
        public virtual Config Config { get; }

        protected ISchemaInitializer SchemaInitializer { get;}
        protected IServiceProvider InternalServiceProvider { get;}

        protected OperationResource(IServiceProvider rootServiceProvider)
        {
            var _serviceScope = ServiceProviderCache.Instance.GetOrAdd(this, rootServiceProvider)
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope();

            var scopedServiceProvider = _serviceScope.ServiceProvider;

            DbSchema = scopedServiceProvider.GetService<IDbSchema>();
            Config = scopedServiceProvider.GetService<Config>();
            AppDbContext = scopedServiceProvider.GetService<AppDbContext>();
            SchemaInitializer = scopedServiceProvider.GetService<ISchemaInitializer>();
            Db = scopedServiceProvider.GetService<IDbAdapter>();

            _connctionStringFactory = scopedServiceProvider.GetService<IConnectionStringFactory>();

            InternalServiceProvider = scopedServiceProvider;
        }

        public virtual void ApplyServices(IServiceCollection services, IServiceProvider rootProvider)
        {
            var options = rootProvider.GetRequiredService<DbContextOptions<AppDbContext>>();

            services.TryAddScoped<AppDbContext>();
            services.TryAddScoped(c => options);

            services.TryAddScoped<DynamicClassBuilder>();
            services.TryAddScoped<IDbAdapter, DbAdapter>();
            services.TryAddScoped<Config>();
            services.TryAddScoped<IDbSchema, DbSchema>();
            services.TryAddScoped<ISchemaInitializer, SchemaInitializer>();
            services.TryAddScoped<IDbTypeMapper, DbTypeMapper>();
            services.TryAddTransient<IConnectionStringFactory, ConnectionStringFactory>();

            var replaceServices = rootProvider.GetService<IReplaceServices<OperationResource>>();
            if (replaceServices != null)
                services = replaceServices.ReplaceInternalServices(rootProvider, services);
        }

        protected virtual IDbAdapter Db { get; }
        public abstract ResourceType ResourceType { get; }


        public void ConfigureAction(Connector connector, OperationType dbAction, string dbObjectName)
        {
            Check.NotNull(connector, nameof(connector));

            Connector = connector;
            DbObjectName = dbObjectName;

            var connectionInfo = _connctionStringFactory.GetConnectionInformation(Connector.ResourceType, Connector.ConnectionStringDecrypted);
            Connector.SetConnectionInfo(connectionInfo);

            SchemaInitializer.ConfigureAction(connector, dbAction);
        }

        public virtual IEnumerable<SchemaDefinition> ListSchemas()
        {
            //Return all tables and the connector their schema belongs too
            return AppDbContext.Connector.Where(c => c.InitialCatalog == DatabaseName)
                                   .OrderBy(c => c.Schema)
                                   .Select(c => new SchemaDefinition
                                   {
                                       Name = c.Schema,
                                       ConnectorId = c.Id,
                                       ConnectorName = c.Name
                                   });

        }


        public virtual IQueryable<object> GetResourceRecords(IDictionary<string, string> queryString)
        {
            Check.NotNull(queryString, nameof(queryString));

            LoadDbAdapter();
            IQueryable<object> list = (IQueryable<object>)Db.GetAll(DbSchema.GetTableName(DatabaseName, SchemaName, DbObjectName));

            list = ExpandUserJoins(list, queryString);

            return list;
        }


        private IQueryable<object> ExpandUserJoins(IQueryable<object> list, IDictionary<string, string> queryString)
        {
            Table outerTable = DbSchema.GetTable(DatabaseName, SchemaName, DbObjectName);
            var userJoins = Config.UserDefinedJoins;

            foreach (var userJoin in userJoins)
            {
                string[] expandTables = queryString.ContainsKey("$expand") ? queryString["$expand"].Replace(" ", "").Split(',') : Array.Empty<string>();

                if (expandTables.Contains(UserJoinListName(userJoin)) || expandTables.Contains(UserJoinReverseListName(userJoin)))
                {

                    string tableName;

                    //Outer columns + nav props
                    string outerColumns = string.Join(",",
                        ReflectionExtensions.UserJoinedColumnSelect(outerTable, list.ElementType, queryString, "outer."));

                    //If outer table is PK get inner from FK table object
                    if (expandTables.Contains(UserJoinListName(userJoin)))
                    {
                        tableName = DbSchema.GetTableName(userJoin.DependentDatabase, userJoin.DependentSchema, userJoin.DependentTable);
                        list = GroupJoinUserJoin(list, outerTable.VariableName, tableName, userJoin.PrincipalColumns, userJoin.DependentColumns, userJoin.Alias, outerColumns);
                    }
                    //Swap FK and PK
                    else
                    {
                        tableName = DbSchema.GetTableName(userJoin.PrincipalDatabase, userJoin.PrincipalSchema, userJoin.PrincipalTable);
                        list = GroupJoinUserJoin(list, outerTable.VariableName, tableName, userJoin.DependentColumns, userJoin.PrincipalColumns, userJoin.Alias, outerColumns);
                    }
                }
                else
                {
                    //Outer columns + nav props
                    string columns = string.Join(",", ReflectionExtensions.UserJoinedColumnSelect(outerTable, list.ElementType, queryString));
                    string navPropName = userJoin.PrincipalTable == outerTable.Name ? UserJoinListName(userJoin) : UserJoinReverseListName(userJoin);

                    list = list.Select($"new ({columns}, Int32?(null) as {navPropName})") as IQueryable<object>;
                }
            }

            return list;
        }

        private IQueryable<object> GroupJoinUserJoin(IQueryable<object> list, string outerTableName, string innerTableName, string outerColumn, string innerColumn, string alias, string outerColumns)
        {
            var inner = (IQueryable<object>)Db.GetWithoutContext(innerTableName, outerTableName);

            //format prefix
            string prefix = $"UJ_{alias}";

            //Build join expression
            string outerKeySelector = FormatJoinObject(outerColumn.Split(','), list.ElementType, "outer");
            string innerKeySelector = FormatJoinObject(innerColumn.Split(','), inner.ElementType, "inner");

            list = list.GroupJoin(inner, outerKeySelector, innerKeySelector,
                    $"new ({outerColumns}, group as  {prefix}ListFrom{innerColumn.Replace(",", "And")})");

            return list;
        }

        private static string FormatJoinObject(string[] columns, Type type, string prefix = "")
        {
            if (columns.Length == 1)
                return AddNullableTypeCast(type, columns[0], prefix);
            else
            {
                return $"new ({string.Join(", ", columns)})";
            }
        }

        private static string AddNullableTypeCast(Type type, string property, string prefix)
        {
            var propertyType = type.GetProperty(property).PropertyType;
            propertyType = Nullable.GetUnderlyingType(propertyType) ?? propertyType; // avoid type becoming null
            if (propertyType.IsValueType)
                return $"{propertyType.Name}?({prefix}.{property})";
            else
                return $"{prefix}.{property}";
        }

        private static string UserJoinListName(UserJoinConfiguration userJoin) => $"{UserJoinPrefix(userJoin)}ListFrom{userJoin.DependentColumns.Replace(",", "And")}";

        private static string UserJoinReverseListName(UserJoinConfiguration userJoin) => $"{UserJoinPrefix(userJoin)}ListFrom{userJoin.PrincipalColumns.Replace(",", "And")}";

        private static string UserJoinPrefix(UserJoinConfiguration userJoin) => "UJ_" + userJoin.Alias;

        /// <summary>
        /// Retrieve single row by primary key
        /// </summary>
        /// <param name="keys">Primary key.</param>
        /// <returns></returns>
        public virtual object GetResourceRecordById(string keys)
        {
            Check.NotNull(keys, nameof(keys));

            this.LoadDbAdapter();
            var record = Db.Find(DbSchema.GetTableName(DatabaseName, SchemaName, DbObjectName), keys);

            return record;
        }

        public virtual TViewModel GetResourceRecordById<TViewModel>(string keys, IDictionary<string, string> queryString)
        {
            Check.NotNull(keys, nameof(keys));
            Check.NotNull(queryString, nameof(queryString));

            this.LoadDbAdapter();
            var list = Db.FilterByKey(DbSchema.GetTableName(DatabaseName, SchemaName, DbObjectName), keys);
            list = ExpandUserJoins(list, queryString);
            var listOfT = list.ProjectTo<TViewModel>();
            return listOfT.Single();
        }

        /// <summary>
        ///  Update record
        /// </summary>
        /// <typeparam name="TViewModel">Model of values.</typeparam>
        /// <param name="value">Instance of model to update.</param>
        /// <param name="keys">Primary Key.</param>
        /// <returns></returns>
        public virtual TViewModel UpdateResourceRecordById<TViewModel>(TViewModel value, string keys) where TViewModel : class
        {
            Check.NotNull(value, nameof(value));
            Check.NotNull(keys, nameof(keys));

            this.LoadDbAdapter();
            var tableVariableName = DbSchema.GetTableName(DatabaseName, SchemaName, DbObjectName);
            var record = Db.Find(tableVariableName, keys);

            if (record != null)
            {
                value.PopulateModel(record);
                Db.Update(record);
                Db.Save();

                //SetValues that shouldn't have changed
                record.PopulateModel(value);

                return value;
            }

            return null;
        }

        /// <summary>
        ///  Update record
        /// </summary>
        /// <param name="value">JSON object representing values</param>
        /// <param name="keys">Primary Key.</param>
        /// <returns>Updated record</returns>
        public virtual object UpdateResourceRecordById(JToken value, string keys)
        {
            Check.NotNull(value, nameof(value));
            Check.NotNull(keys, nameof(keys));

            this.LoadDbAdapter();
            var tableVariableName = DbSchema.GetTableName(DatabaseName, SchemaName, DbObjectName);
            var record = Db.Find(tableVariableName, keys);

            if (record != null)
            {
                value.PopulateObjectFromJToken(record);
                Db.Update(record);
                Db.Save();

                return record;
            }

            return null;
        }

        /// <summary>
        ///  Insert a recored
        /// </summary>
        /// <typeparam name="TViewModel">Model of values.</typeparam>
        /// <param name="value">Instance of model to update.</param>
        /// <returns></returns>
        public virtual TViewModel CreateNewResourceRecord<TViewModel>(TViewModel value)
        {
            Check.NotNull(value, nameof(value));

            this.LoadDbAdapter();
            var tableVariableName = DbSchema.GetTableName(DatabaseName, SchemaName, DbObjectName);
            var record = Db.NewInstance(tableVariableName);
            value.PopulateModel(record);

            Db.Add(record);
            Db.Save();

            //SetValues that shouldn't have changed
            record.PopulateModel(value);

            return value;
        }

        public virtual object CreateNewResourceRecord(JToken value)
        {
            Check.NotNull(value, nameof(value));

            this.LoadDbAdapter();
            var tableVariableName = DbSchema.GetTableName(DatabaseName, SchemaName, DbObjectName);
            var record = Db.NewInstance(tableVariableName);
            value.PopulateObjectFromJToken(record);

            Db.Add(record);
            Db.Save();

            return record;
        }

        public virtual object DeleteResourceRecordById(string keys)
        {
            this.LoadDbAdapter();
            var record = Db.Find(DbSchema.GetTableName(DatabaseName, SchemaName, DbObjectName), keys);
            var recordResults = new Dictionary<string, object>();
            if (record != null)
            {
                Db.Delete(record);
                Db.Save();

                recordResults["data"] = record;
                recordResults["results"] = "Record was successfully deleted.";
                return recordResults;
            }

            recordResults["data"] = null;
            recordResults["results"] = "No records were deleted.";
            return recordResults;
        }

        public virtual IQueryable<object> GetViewRecords()
        {
            this.LoadDbAdapter();
            View view = DbSchema.GetView(SchemaName, DbObjectName);
            string viewColumns = $"new ({string.Join(",", view.Columns.Select(c => c.ColumnName))})";

            var queryWithPKColumn = (IQueryable<object>)Db.GetAll(view.VariableName);
            queryWithPKColumn = queryWithPKColumn.Select(viewColumns) as IQueryable<object>;
            return queryWithPKColumn;

        }

        /// <summary>
        /// Detailed information about a table column
        /// </summary>
        /// <param name="columnName">Column to look up</param>
        /// <returns></returns>
        public virtual ColumnDefinition GetColumnDefinition(string columnName)
        {
            this.LoadDbAdapter();
            return DbSchema.Columns
                                     .Where(c => columnName.Equals(c.ColumnName, StringComparison.OrdinalIgnoreCase) && DbObjectName.Equals(c.Table.Name, StringComparison.OrdinalIgnoreCase))
                                     .Select(c => new ColumnDefinition
                                     {
                                         Name = c.ColumnName,
                                         Type = c.ColumnType,
                                         Length = c.ColumnLength,
                                         IsComputed = c.IsComputed,
                                         IsNullable = c.ColumnIsNullable,
                                         IsPrimaryKey = c.IsPK,
                                         IsForigenKey = c.IsFK,
                                         ReferencedDatabase = c.ReferencedDatabase,
                                         ReferencedTable = c.ReferencedTable,
                                         ReferencedColumn = c.ReferencedColumn,
                                         ReferencedSchema = c.ReferencedSchema,
                                         IsPrimaryKeyIdentity = c.PKIsIdentity
                                     })
                                     .FirstOrDefault();
        }

        public virtual SchemaDefinition GetSchemaDefinition()
        {
            this.LoadSchema();
            return new SchemaDefinition
            {
                Tables = DbSchema.Tables.Select(c => c.Name),
                Views = DbSchema.Views.Select(c => c.Name),
                StoredProcedures = DbSchema.StoredProcedures.Select(c => c.Name),
                ConnectorId = Connector.Id,
                ConnectorName = Connector.Name,
                Name = Connector.Schema,
                DbName = Connector.InitialCatalog
            };
        }

        public virtual StoredProcedureDefinition GetStoredProcedureDefinition()
        {
            this.LoadProc(SchemaName, DbObjectName);
            return DbSchema.StoredProcedures
                                     .Where(c => DbObjectName.Equals(c.Name, StringComparison.OrdinalIgnoreCase))
                                     .Select(c => new StoredProcedureDefinition
                                     {
                                         Name = c.Name,
                                         Parameters = c.Parameters.Select(d => new StoredProcedureParameterDefinition
                                         {
                                             Name = d.Name,
                                             Type = d.Type,
                                             IsOutput = d.IsOutput,
                                             IsNullable = d.IsNullable
                                         })
                                     })
                                     .FirstOrDefault();
        }

        public virtual StoredProcedureParameterDefinition GetStoredProcedureDefinition(string parameterName)
        {
            this.LoadProc(SchemaName, DbObjectName);
            return DbSchema.StoredProcedures
                                     .Where(c => DbObjectName.Equals(c.Name, StringComparison.OrdinalIgnoreCase))
                                     .SelectMany(c => c.Parameters.Select(d => new StoredProcedureParameterDefinition
                                     {
                                         Name = d.Name,
                                         Type = d.Type,
                                         IsOutput = d.IsOutput,
                                         IsNullable = d.IsNullable
                                     })
                                     ).FirstOrDefault(c => c.Name.Equals(parameterName, StringComparison.OrdinalIgnoreCase));
        }

        public virtual TableDefinition GetTableDefinition()
        {
            this.LoadDbAdapter();
            return DbSchema.Tables
                                     .Where(c => DbObjectName.Equals(c.Name, StringComparison.OrdinalIgnoreCase))
                                     .Select(c => new TableDefinition
                                     {
                                         Name = c.Name,
                                         ConnectorId = Connector.Id,
                                         ConnectorName = Connector.Name,
                                         SchemaName = c.Schema,
                                         DbName = Connector.InitialCatalog,
                                         Columns = c.Columns.Select(d => new ColumnDefinition
                                         {
                                             Name = d.ColumnName,
                                             Type = d.ColumnType,
                                             Length = d.ColumnLength,
                                             IsComputed = d.IsComputed,
                                             IsNullable = d.ColumnIsNullable,
                                             IsPrimaryKey = d.IsPK,
                                             IsForigenKey = d.IsFK,
                                             ReferencedTable = d.ReferencedTable,
                                             ReferencedColumn = d.ReferencedColumn,
                                             ReferencedSchema = d.ReferencedSchema,
                                             ReferencedDatabase = d.ReferencedDatabase,
                                             IsPrimaryKeyIdentity = d.PKIsIdentity
                                         })
                                     })
                                     .FirstOrDefault();
        }

        public virtual void LoadProc(string schemaName, string storedProcedureName)
        {
            Config.FilterSchema = schemaName;
            Config.IncludedStoredProcedure = storedProcedureName;
            SchemaInitializer.Initilize();
        }

        public virtual void LoadDbAdapter()
        {
            Config.FilterSchema = SchemaName;
            Config.IncludedTable = DbObjectName;
            Config.LoadUserDefinedTables(Connector, AppDbContext);
            Config.DatabaseConnectorName = Connector.Name;
            SchemaInitializer.Initilize();
        }

        public virtual void LoadSchema()
        {
            Config.FilterSchema = SchemaName;
            Config.DatabaseConnectorName = Connector.Name;
            SchemaInitializer.Initilize();
        }
    }
}