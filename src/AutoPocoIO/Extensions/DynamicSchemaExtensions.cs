using AutoPocoIO.DynamicSchema.Db;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.Exceptions;
using System;
using System.Linq;
using System.Reflection;

namespace AutoPocoIO.Extensions
{
    public static class DynamicSchemaExtensions
    {
        public static Table GetTable(this IDbSchema dbSchema, string databaseName, string schemaName, string tableName)
        {
            Check.NotNull(dbSchema, nameof(dbSchema));
            Check.NotNull(databaseName, nameof(databaseName));
            Check.NotNull(schemaName, nameof(schemaName));
            Check.NotNull(tableName, nameof(tableName));

            try
            {
                return dbSchema.Tables.First(c => c.Name.ToUpperInvariant() == tableName.ToUpperInvariant()
                                                        && c.Schema.ToUpperInvariant() == schemaName.ToUpperInvariant()
                                                        && c.Database.ToUpperInvariant() == databaseName.ToUpperInvariant());
            }
            catch (InvalidOperationException)
            {
                throw new TableNotFoundException(databaseName, schemaName, tableName);
            }
        }

        internal static Table GetTableOrNull(this IDbSchema dbSchema, string databaseName, string schemaName, string tableName)
        {
            return dbSchema.Tables.FirstOrDefault(c => c.Name.ToUpperInvariant() == tableName.ToUpperInvariant()
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

        internal static View GetView(this IDbSchema dbSchema, string schemaName, string viewName)
        {
            try
            {
                return dbSchema.Views.First(c => c.Name.ToUpperInvariant() == viewName.ToUpperInvariant() && c.Schema.ToUpperInvariant() == schemaName.ToUpperInvariant());
            }
            catch (InvalidOperationException)
            {
                throw new ViewNotFoundException(schemaName, viewName);
            }
        }

        public static StoredProcedure GetStoredProcedure(this IDbSchema dbSchema, string schemaName, string sprocName)
        {
            Check.NotNull(dbSchema, nameof(dbSchema));

            try
            {
                return dbSchema.StoredProcedures.First(c => c.Schema.ToUpperInvariant() == schemaName.ToUpperInvariant() && c.Name.ToUpperInvariant() == sprocName.ToUpperInvariant());
            }
            catch (InvalidOperationException)
            {
                throw new StoreProcedureNotFoundException(schemaName, sprocName);
            }
        }

        internal static object InvokeWithException(this MethodBase method, object obj, object[] parameters)
        {
            try
            {
                return method.Invoke(obj, parameters);
            } catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }
    }
}