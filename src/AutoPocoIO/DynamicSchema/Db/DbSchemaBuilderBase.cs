using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.DynamicSchema.Util;
using AutoPocoIO.Exceptions;
using AutoPocoIO.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Globalization;

namespace AutoPocoIO.DynamicSchema.Db
{
    /// <summary>
    /// Base class to populate schema information
    /// </summary>
    public abstract class DbSchemaBuilderBase : IDbSchemaBuilder
    {
        private readonly IDbSchema _dbSchema;
        private readonly IDbTypeMapper _typeMapper;

        /// <summary>
        /// Initalize schmea builder
        /// </summary>
        /// <param name="config">Resource configuration</param>
        /// <param name="dbSchema">Schmea infomration to populate</param>
        /// <param name="typeMapper">How to map db types to c# types</param>
        public DbSchemaBuilderBase(
            Config config,
            IDbSchema dbSchema,
            IDbTypeMapper typeMapper)
        {
            Config = config;
            _dbSchema = dbSchema;
            _typeMapper = typeMapper;
        }


        /// <summary>
        /// Create command to get column information.
        /// </summary>
        /// <param name="dbConnection">Database to pull the information from.</param>
        /// <returns></returns>
        protected abstract IDbCommand BuildColumnsCommand(IDbConnection dbConnection);
        /// <summary>
        /// Create command to get stored procedure information.
        /// </summary>
        /// <param name="dbConnection">Database to pull the information from.</param>
        /// <returns></returns>
        protected abstract IDbCommand BuildStoredProcedureCommand(IDbConnection dbConnection);
        /// <summary>
        /// Create command to get table and view information.
        /// </summary>
        /// <param name="dbConnection">Database to pull the information from.</param>
        /// <returns></returns>
        protected abstract IDbCommand BuildTablesViewsCommand(IDbConnection dbConnection);

        ///<inheritdoc/>
        public abstract ResourceType ResourceType { get; }
        ///<inheritdoc/>
        public abstract IDbConnection CreateConnection();
        ///<inheritdoc/>
        public abstract IDbConnection CreateConnection(string connectionString);
        ///<inheritdoc/>
        public abstract DbContextOptions CreateDbContextOptions();

        /// <summary>
        /// Resource configuration
        /// </summary>
        protected Config Config { get; }

        ///<inheritdoc/>
        public virtual DataTable ExecuteSchemaCommand(IDbCommand command)
        {
            Check.NotNull(command, nameof(command));

            DataTable dtSchema = new DataTable();
            command.Connection.SafeDbConnectionOpen(Config.DatabaseConnectorName);
            var reader = command.ExecuteReader();
            dtSchema.Load(reader);

            return dtSchema;
        }

        ///<inheritdoc/>
        public virtual void GetColumns()
        {
            if (string.IsNullOrEmpty(Config.IncludedStoredProcedure))
            {
                DataTable dtSchema = null;

                foreach (var connections in Config.UsedConnectors)
                {
                    using (var connection = CreateConnection(connections))
                    {
                        using (var command = BuildColumnsCommand(connection))
                        {
                            if (dtSchema == null)
                                dtSchema = ExecuteSchemaCommand(command);
                            else
                                dtSchema.Merge(ExecuteSchemaCommand(command));
                        }
                    }
                }

                Table currentTable = null;
                foreach (DataRow row in dtSchema.Rows)
                {
                    currentTable = AddTableAndColumns(row, currentTable);
                }
            }
        }

        ///<inheritdoc/>
        public virtual void GetStoredProcedures()
        {
            DataTable dtSchema = null;

            using (var connection = CreateConnection())
            {
                using (var command = BuildStoredProcedureCommand(connection))
                {
                    dtSchema = ExecuteSchemaCommand(command);
                }
            }

            StoredProcedure currentProcedure = null;
            foreach (DataRow row in dtSchema.Rows)
            {
                currentProcedure = AddStoreProcedure(row, currentProcedure);
            }
        }

        ///<inheritdoc/>
        public virtual void GetTableViews()
        {
            DataTable dtSchema = null;
            using (var connection = CreateConnection())
            {
                using (var command = BuildTablesViewsCommand(connection))
                {
                    dtSchema = ExecuteSchemaCommand(command);
                }
            }
            Table currentTable = null;
            foreach (DataRow row in dtSchema.Rows)
            {
                currentTable = AddTableAndColumns(row, currentTable);
            }
        }

        private Table AddTableAndColumns(DataRow row, Table currentTable)
        {
            var objectType = row["ObjectType"].ToString().Trim();
            DBOjectType currentObjectType = objectType.SetObjectType();


            if (currentTable == null || $"{currentTable.Database}.{currentTable.Schema}.Tables{currentTable.Name}" !=
                 $"{row["DatabaseName"]}.{row["TableSchema"]}.Tables{row["TableName"]}")
            {
                switch (currentObjectType)
                {
                    case DBOjectType.Table:
                        currentTable = new Table()
                        {
                            Schema = row["TableSchema"].ToString(),
                            Name = row["TableName"].ToString(),
                            Database = row["DatabaseName"].ToString(),
                            PrimaryKeys = row["PKColumnName"].ToString()
                        };

                        _dbSchema.Tables.Add(currentTable);
                        break;
                    case DBOjectType.View:
                        currentTable = new View()
                        {
                            Schema = row["TableSchema"].ToString(),
                            Name = row["TableName"].ToString(),
                            Database = row["DatabaseName"].ToString(),
                            PrimaryKeys = row["PKColumnName"].ToString()
                        };
                        _dbSchema.Views.Add((View)currentTable);
                        break;
                }
            }

            Column column = new Column
            {
                TableSchema = row["TableSchema"].ToString(),
                TableName = row["TableName"].ToString(),
                ColumnName = Config.PropertyPreFixName + row["ColumnName"].ToString(),
                ColumnType = row["ColumnType"].ToString(),
                ColumnLength = Convert.ToInt32(row["ColumnLength"], CultureInfo.InvariantCulture),
                ColumnIsNullable = Convert.ToBoolean(row["ColumnIsNullable"], CultureInfo.InvariantCulture),
            };

            //Add PK/FK Info to column
            if (currentObjectType == DBOjectType.Table)
            {
                column.PKName = row["PKName"].ToString();
                column.PKPosition = Convert.ToInt32(row["PKPosition"], CultureInfo.InvariantCulture);
                column.PKIsIdentity = Convert.ToBoolean(row["PKIsIdentity"], CultureInfo.InvariantCulture);
                column.IsComputed = Convert.ToBoolean(row["IsComputed"], CultureInfo.InvariantCulture);

                if (!string.IsNullOrEmpty(row["FKName"].ToString()))
                    AddFKColumn(row, column);
            }

            column.DataType = _typeMapper.DBTypeToDataType(column);

            switch (currentObjectType)
            {
                case DBOjectType.Table:
                    {
                        column.Table = currentTable;
                        currentTable.Columns.Add(column);
                        break;
                    }
                case DBOjectType.View:
                    column.View = (View)currentTable;
                    currentTable.Columns.Add(column);
                    break;
            }

            _dbSchema.Columns.Add(column);

            return currentTable;
        }

        /// <summary>
        /// Add Foriegn key information to the column
        /// </summary>
        /// <param name="row">Result row from database</param>
        /// <param name="column">Target column</param>
        protected virtual void AddFKColumn(DataRow row, Column column)
        {
            Check.NotNull(row, nameof(row));
            Check.NotNull(column, nameof(column));

            column.FKName = row["FKName"].ToString();
            column.ReferencedDatabase = row["DatabaseName"].ToString();
            column.ReferencedSchema = row["ReferencedSchema"].ToString();
            column.ReferencedTable = row["ReferencedTable"].ToString();
            column.ReferencedColumn = row["ReferencedColumn"].ToString();
        }

        private StoredProcedure AddStoreProcedure(DataRow row, StoredProcedure storedProcedure)
        {
            if (storedProcedure == null || string.Format(CultureInfo.InvariantCulture, "{0}.SProc{1}", storedProcedure.Schema, storedProcedure.Name) != string.Format(CultureInfo.InvariantCulture, "{0}.SProc{1}", row["ProcSchema"].ToString(), row["ProcName"].ToString()))
            {
                storedProcedure = new StoredProcedure
                {
                    Schema = row["ProcSchema"].ToString(),
                    Name = row["ProcName"].ToString(),
                    Database = row["DatabaseName"].ToString().Trim()
                };
                _dbSchema.StoredProcedures.Add(storedProcedure);
            }

            if (!string.IsNullOrEmpty(row["ParamName"].ToString()))
            {
                DBParameter parameter = new DBParameter
                {
                    Name = row["ParamName"].ToString(),
                    Type = row["ParamType"].ToString(),
                    IsOutput = Convert.ToBoolean(row["IsOutput"], CultureInfo.InvariantCulture),
                    IsNullable = Convert.ToBoolean(row["IsNullable"], CultureInfo.InvariantCulture)
                };

                storedProcedure.Parameters.Add(parameter);
            }

            return storedProcedure;

        }
    }
}
