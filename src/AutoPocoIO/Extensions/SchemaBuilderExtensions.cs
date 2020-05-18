using AutoPocoIO.Constants;
using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.Exceptions;
using System;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace AutoPocoIO.Extensions
{
    internal static class SchemaBuilderExtensions
    {
        /// <summary>
        /// Convert db result to DbObject Enum
        /// </summary>
        /// <param name="objectType">string value from Db</param>
        ///<exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        public static DBOjectTypes SetObjectType(this string objectType)
        {
            switch (objectType)
            {
                case "U":
                    return DBOjectTypes.Table;
                case "V":
                    return DBOjectTypes.View;
                default:
                    throw new ArgumentOutOfRangeException(nameof(objectType), ExceptionMessages.CharToDbObjecType);
            }
        }

        public static void SafeDbConnectionOpen(this IDbConnection db, string connectorName)
        {
            try
            {
                db.Open();
            }
            catch (DbException)
            {
                throw new OpenConnectorException(connectorName);
            }
        }
        public static string GenerateFKName(this Table table, Column fkColumn)
        {
            return string.Join("And", table.Columns.Where(c => c.FKName == fkColumn.FKName));
        }
    }
}