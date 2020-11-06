using AutoPocoIO.Context;
using AutoPocoIO.CustomAttributes;
using AutoPocoIO.DynamicSchema.Db;
using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.DynamicSchema.Runtime;
using AutoPocoIO.DynamicSchema.Util;
using AutoPocoIO.EntityConfiguration;
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
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.AutoPoco;
using System.Linq.Dynamic.Core;
using System.Reflection;

namespace AutoPocoIO.Resources
{
    public abstract class OperationResource : IOperationResource
    {
        private readonly AppDbContext AppDbContext;
        private readonly IConnectionStringFactory _connctionStringFactory;

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

        ///<inheritdoc/>
        public virtual Connector Connector { get; protected set; }
        ///<inheritdoc/>
        public string DatabaseName => Connector.InitialCatalog;
        ///<inheritdoc/>
        public string SchemaName => Connector.Schema;
        ///<inheritdoc/>
        public virtual string DbObjectName { get; private set; }
        ///<inheritdoc/>
        public virtual IDbSchema DbSchema { get; }
        ///<inheritdoc/>
        public virtual Config Config { get; }
        ///<inheritdoc/>
        public abstract string ResourceType { get; }

        protected ISchemaInitializer SchemaInitializer { get; }
        protected IServiceProvider InternalServiceProvider { get; }
        protected virtual IDbAdapter Db { get; }

        public abstract IDictionary<string, object> ExecuteProc(IDictionary<string, object> parameterDictionary);

        ///<inheritdoc/>
        public virtual void ApplyServices(IServiceCollection services, IServiceProvider rootProvider)
        {
            var options = rootProvider.GetRequiredService<DbContextOptions<AppDbContext>>();
            var contextConfig = rootProvider.GetService<IContextEntityConfiguration>();

            services.TryAddScoped<AppDbContext>();
            services.TryAddScoped(c => options);
            if(contextConfig != null)
                services.TryAddTransient(c => contextConfig);

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

        ///<inheritdoc/>
        public void ConfigureAction(Connector connector, OperationType dbAction, string dbObjectName)
        {
            Check.NotNull(connector, nameof(connector));

            Connector = connector;
            DbObjectName = dbObjectName;

            var connectionInfo = _connctionStringFactory.GetConnectionInformation(Connector.ResourceType, Connector.ConnectionStringDecrypted);
            Connector.SetConnectionInfo(connectionInfo);

            SchemaInitializer.ConfigureAction(connector, dbAction);
        }

        ///<inheritdoc/>
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

        ///<inheritdoc/>
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

        ///<inheritdoc/>
        public virtual object GetResourceRecordById(object[] keys)
        {
            Check.NotNull(keys, nameof(keys));

            this.LoadDbAdapter();
            var record = Db.Find(DbSchema.GetTableName(DatabaseName, SchemaName, DbObjectName), keys);

            return record;
        }

        ///<inheritdoc/>
        public virtual TViewModel GetResourceRecordById<TViewModel>(object[] keys, IDictionary<string, string> queryString)
        {
            Check.NotNull(keys, nameof(keys));
            Check.NotNull(queryString, nameof(queryString));

            this.LoadDbAdapter();
            var list = Db.FilterByKey(DbSchema.GetTableName(DatabaseName, SchemaName, DbObjectName), keys);
            list = ExpandUserJoins(list, queryString);
            var listOfT = list.ProjectTo<TViewModel>();
            return listOfT.Single();
        }

        ///<inheritdoc/>
        public virtual IQueryable<object> GetResourceRecords(IDictionary<string, string> queryString)
        {
            Check.NotNull(queryString, nameof(queryString));

            LoadDbAdapter();
            IQueryable<object> list = (IQueryable<object>)Db.GetAll(DbSchema.GetTableName(DatabaseName, SchemaName, DbObjectName));

            list = ExpandNavProperties(list, queryString);
            list = ExpandUserJoins(list, queryString);

            return list;
        }

        ///<inheritdoc/>
        public virtual TViewModel UpdateResourceRecordById<TViewModel>(TViewModel value, object[] keys) where TViewModel : class
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

        ///<inheritdoc/>
        public virtual object UpdateResourceRecordById(JToken value, object[] keys)
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
        public object[] GetPrimaryKeys(object value)
        {
            this.LoadDbAdapter();
            var tableVariableName = DbSchema.GetTableName(DatabaseName, SchemaName, DbObjectName);
            if (value is JToken token)
            {
                var map = Db.NewInstance(tableVariableName);
                token.PopulateObjectFromJToken(map);
                return Db.MapPrimaryKey(map).Select(c => c.ToString()).ToArray();
            }
            else
            {
                Db.SetupDataContext(tableVariableName);
                return Db.MapPrimaryKey(value).Select(c => c.ToString()).ToArray();
            }
        }

        ///<inheritdoc/>
        public virtual object DeleteResourceRecordById(object[] keys)
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
        ///<inheritdoc/>
        public virtual IQueryable<object> GetViewRecords()
        {
            this.LoadDbAdapter();
            View view = DbSchema.GetView(SchemaName, DbObjectName);
            string viewColumns = $"new ({string.Join(",", view.Columns.Select(c => c.ColumnName))})";

            var queryWithPKColumn = (IQueryable<object>)Db.GetAll(view.VariableName);
            queryWithPKColumn = queryWithPKColumn.Select(viewColumns) as IQueryable<object>;
            return queryWithPKColumn;

        }

        ///<inheritdoc/>
        public virtual IEnumerable<string> ListSchemas()
        {
            SchemaInitializer.FindSchemas();
            return DbSchema.SchemaNames;

        }
        ///<inheritdoc/>
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
        ///<inheritdoc/>
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
        ///<inheritdoc/>
        public virtual StoredProcedureDefinition GetStoredProcedureDefinition()
        {
            this.LoadProc();
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
        ///<inheritdoc/>
        public virtual StoredProcedureParameterDefinition GetStoredProcedureDefinition(string parameterName)
        {
            this.LoadProc();
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
        ///<inheritdoc/>
        public virtual TableDefinition GetTableDefinition()
        {
            this.LoadDbAdapter();
            TableDefinition definition = DbSchema.Tables
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


            definition.NavigationProperties = ListNavigationProperties();
            return definition;
        }

        protected virtual IEnumerable<NavigationPropertyDefinition> ListNavigationProperties()
        {
            Db.SetupDataContext(DbSchema.GetTableName(DatabaseName, SchemaName, DbObjectName));

            var properties = Db.DbSetEntityType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
            var fkAttrs = properties.Select(c => new { Attr = c.GetCustomAttribute<ForeignKeyAttribute>(), Name = c.Name })
                                   .Where(c => c.Attr != null);

            var inverseAttrs = properties.Select(c => new { Attr = c.GetCustomAttribute<InversePropertyAttribute>(), Name = c.Name })
                                    .Where(c => c.Attr != null);

            foreach (var property in properties)
            {
                if (property.PropertyType.IsClass && property.PropertyType != typeof(string)
                    && property.GetCustomAttribute<ReferencedDbObjectAttribute>() != null)
                {
                    NavigationPropertyDefinition navigationProperty = new NavigationPropertyDefinition()
                    {
                        Name = property.Name,
                        ReferencedSchema = property.GetCustomAttribute<ReferencedDbObjectAttribute>().SchemaName,
                        ReferencedTable = property.GetCustomAttribute<ReferencedDbObjectAttribute>().TableName,

                    };
                    if (typeof(System.Collections.IEnumerable).IsAssignableFrom(property.PropertyType))
                    {
                        navigationProperty.Relationship = "Many to 1";
                    }
                    else
                    {
                        navigationProperty.Relationship = "1 to Many";

                    }

                    //Self referencing fk
                    if (property.GetCustomAttribute<InversePropertyAttribute>() == null)
                    {
                        navigationProperty.FromProperty = DbSchema.Tables.FirstOrDefault(c => c.Name == property.GetCustomAttribute<ReferencedDbObjectAttribute>().TableName).PrimaryKeys;
                        navigationProperty.ToProperty = string.Join(", ", fkAttrs.Where(c => c.Attr.Name == property.Name)
                                                                .Select(c => c.Name));
                    }
                    else
                    {
                        var inverseProperty = property.GetCustomAttribute<InversePropertyAttribute>().Property;


                        navigationProperty.ToProperty = DbSchema.Tables.FirstOrDefault(c => c.Name == property.GetCustomAttribute<ReferencedDbObjectAttribute>().TableName).PrimaryKeys;
                        navigationProperty.FromProperty = string.Join(", ", fkAttrs.Where(c => c.Attr.Name == inverseProperty)
                                                               .Select(c => c.Name));
                    }

                    yield return navigationProperty;


                   
                }
            }

            var userJoins = Config.UserDefinedJoins;

            foreach (var userJoin in userJoins)
            {
                yield return new NavigationPropertyDefinition()
                {
                    Name = UserJoinListName(userJoin),
                    ReferencedSchema = userJoin.DependentSchema,
                    ReferencedTable = userJoin.DependentTable,
                    FromProperty = userJoin.PrincipalColumns,
                    ToProperty = userJoin.DependentColumns,
                    IsUserDefinied = true,
                    Relationship = "Many to Many"
                };
            }

        }
        ///<inheritdoc/>
        public virtual void LoadProc()
        {
            Config.FilterSchema = SchemaName;
            Config.IncludedStoredProcedure = DbObjectName;
            SchemaInitializer.Initilize();
        }
        ///<inheritdoc/>
        public virtual void LoadDbAdapter()
        {
            Config.FilterSchema = SchemaName;
            Config.IncludedTable = DbObjectName;
            Config.LoadUserDefinedTables(Connector, AppDbContext);
            Config.DatabaseConnectorName = Connector.Name;
            SchemaInitializer.Initilize();
        }
        ///<inheritdoc/>
        public virtual void LoadSchema()
        {
            Config.FilterSchema = SchemaName;
            Config.DatabaseConnectorName = Connector.Name;
            SchemaInitializer.Initilize();
        }

        private IQueryable<object> ExpandNavProperties(IQueryable<object> list, IDictionary<string, string> queryString)
        {
            var userJoins = Config.UserDefinedJoins;
            List<string> expandTables = queryString.ContainsKey("$expand") ? queryString["$expand"].Replace(" ", "").Split(',').ToList() : new List<string>();
            foreach (var userJoin in userJoins)
            {
                //Not a user join
                if (expandTables.Contains(UserJoinListName(userJoin)))
                    expandTables.Remove(UserJoinListName(userJoin));
                else if (expandTables.Contains(UserJoinReverseListName(userJoin)))
                    expandTables.Remove(UserJoinReverseListName(userJoin));
            }

            foreach(var nav in expandTables)
            {
                list = list.Include(nav);
            }
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

            list = list.LeftJoin(inner, outerKeySelector, innerKeySelector,
                    $"new ({outerColumns}, group as  {prefix}ListFrom{innerColumn.Replace(",", "And")})")
                    .AsQueryable();

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

   
    }
}