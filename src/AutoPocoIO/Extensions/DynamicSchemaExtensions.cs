using AutoPocoIO.DynamicSchema.Db;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.Exceptions;
using System;
using System.Linq;

namespace AutoPocoIO.Extensions
{
    public static class DynamicSchemaExtensions
    {
        public static Table GetTable(this IDbSchema dbSchemaBuilder, string databaseName, string schemaName, string tableName)
        {
            try
            {
                return dbSchemaBuilder.Tables.First(c => c.Name.ToUpperInvariant() == tableName.ToUpperInvariant()
                                                        && c.Schema.ToUpperInvariant() == schemaName.ToUpperInvariant()
                                                        && c.Database.ToUpperInvariant() == databaseName.ToUpperInvariant());
            }
            catch (InvalidOperationException)
            {
                throw new TableNotFoundException(databaseName, schemaName, tableName);
            }
        }

        internal static Table GetTableOrNull(this IDbSchema dbSchemaBuilder, string databaseName, string schemaName, string tableName)
        {
            return dbSchemaBuilder.Tables.FirstOrDefault(c => c.Name.ToUpperInvariant() == tableName.ToUpperInvariant()
                                                            && c.Schema.ToUpperInvariant() == schemaName.ToUpperInvariant()
                                                             && c.Database.ToUpperInvariant() == databaseName.ToUpperInvariant());
        }

        internal static string GetTableName(this IDbSchema db, string databaseName, string schemaName, string tableName)
        {
            return db.GetTable(databaseName, schemaName, tableName).VariableName;
        }

        internal static string GetTableNameOrNull(this IDbSchema db, string databaseName, string schemaName, string tableName)
        {
            return db.GetTableOrNull(databaseName, schemaName, tableName)?.VariableName;
        }

        internal static View GetView(this IDbSchema dbSchemaBuilder, string schemaName, string viewName)
        {
            try
            {
                return dbSchemaBuilder.Views.First(c => c.Name.ToUpperInvariant() == viewName.ToUpperInvariant() && c.Schema.ToUpperInvariant() == schemaName.ToUpperInvariant());
            }
            catch (InvalidOperationException)
            {
                throw new ViewNotFoundException(schemaName, viewName);
            }
        }

        public static StoredProcedure GetStoredProcedure(this IDbSchema dbSchemaBuilder, string schemaName, string sprocName)
        {
            Check.NotNull(dbSchemaBuilder, nameof(dbSchemaBuilder));

            try
            {
                return dbSchemaBuilder.StoredProcedures.First(c => c.Schema.ToUpperInvariant() == schemaName.ToUpperInvariant() && c.Name.ToUpperInvariant() == sprocName.ToUpperInvariant());
            }
            catch (InvalidOperationException)
            {
                throw new StoreProcedureNotFoundException(schemaName, sprocName);
            }
        }
    }
}