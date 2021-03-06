﻿using AutoPocoIO.DynamicSchema.Db;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.Exceptions;
using System;
using System.Linq;
using System.Reflection;

namespace AutoPocoIO.Extensions
{
    /// <summary>
    /// Look up database objects in the schema.
    /// </summary>
    public static class DynamicSchemaExtensions
    {
        /// <summary>
        /// Find a table in the schema.
        /// </summary>
        /// <param name="dbSchema">Schema definition to search.</param>
        /// <param name="databaseName">Target database.</param>
        /// <param name="schemaName">Target schema.</param>
        /// <param name="tableName">Target table.</param>
        /// <returns>The found table definition.</returns>
        /// <exception cref="TableNotFoundException"/>
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

        /// <summary>
        /// Find a strored procedure in the schema.
        /// </summary>
        /// <param name="dbSchema">Schema definition to search.</param>
        /// <param name="schemaName">Target schema.</param>
        /// <param name="sprocName"></param>
        /// <returns>The found schema definition</returns>
        /// <exception cref="StoreProcedureNotFoundException"/>
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

        internal static object InvokeWithException(this MethodBase method, object obj, object[] parameters)
        {
            try
            {
                return method.Invoke(obj, parameters);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }
    }
}