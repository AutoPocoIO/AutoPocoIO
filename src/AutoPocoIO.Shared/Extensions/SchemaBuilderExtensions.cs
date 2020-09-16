using AutoPocoIO.Constants;
using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.Exceptions;
using System;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;

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
        public static DBOjectType SetObjectType(this string objectType)
        {
            switch (objectType)
            {
                case "U":
                    return DBOjectType.Table;
                case "V":
                    return DBOjectType.View;
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
            var columns = table.Columns.Where(c => !string.IsNullOrEmpty(c.FKName) &&
                                            c.FKName.Equals(fkColumn.FKName, StringComparison.OrdinalIgnoreCase))
                                .Select(c => c.ColumnName.UppercaseFirst());
            return string.Join("And", columns);
        }
        public static void AppendUppercaseFirst(this StringBuilder builder, string value)
        {
            builder.Append(value.UppercaseFirst());
        }

        private static string UppercaseFirst(this string value)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            return char.ToUpper(value[0], CultureInfo.InvariantCulture) + value.Substring(1);
        }
    }
}