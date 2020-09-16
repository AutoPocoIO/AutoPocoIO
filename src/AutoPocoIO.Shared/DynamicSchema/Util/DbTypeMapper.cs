using AutoPocoIO.Constants;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.Exceptions;
using System;

namespace AutoPocoIO.DynamicSchema.Util
{

    /// <summary>
    /// Convert Database type to C# type.
    /// </summary>
    public class DbTypeMapper : IDbTypeMapper
    {
        ///<inheritdoc/>
        public virtual DataType DBTypeToDataType(Column column)
        {
            Check.NotNull(column, nameof(column));

            string dataType = column.ColumnType.ToUpperInvariant();
            DataType ret = new DataType
            {
                DbType = column.ColumnType
            };

            try
            {
                if (column.ColumnIsNullable)
                {
                    ret.DbType += "(nullable)";
                }

                switch (dataType.ToUpperInvariant())
                {
                    case "UNIQUEIDENTIFIER":
                        ret.SystemType = typeof(Guid?);
                        break;
                    case "XML":
                    case "NVARCHAR":
                    case "CHAR":
                    case "NCHAR":
                    case "TEXT":
                    case "NTEXT":
                    case "VARCHAR":
                    case "VARCHAR2":
                    case "SYSNAME":
                        ret.SystemType = typeof(string);
                        break;
                    case "MONEY":
                    case "SMALLMONEY":
                    case "NUMERIC":
                    case "DECIMAL":
                    case "NUMBER":
                        ret.SystemType = typeof(decimal?);
                        break;
                    case "TINYINT":
                        ret.SystemType = typeof(byte?);
                        break;
                    case "SMALLINT":
                        ret.SystemType = typeof(short?);
                        break;
                    case "INT":
                        ret.SystemType = typeof(int?);
                        break;
                    case "BIGINT":
                        ret.SystemType = typeof(long?);
                        break;
                    case "FLOAT":
                        ret.SystemType = typeof(double?);
                        break;
                    case "REAL":
                        ret.SystemType = typeof(float?);
                        break;
                    case "DATE":
                    case "SMALLDATETIME":
                    case "DATETIME":
                    case "DATETIME2":
                        ret.SystemType = typeof(DateTime?);
                        break;
                    case "DATETIMEOFFSET":
                        ret.SystemType = typeof(DateTimeOffset?);
                        break;
                    case "TIMESTAMP":
                    case "IMAGE":
                    case "VARBINARY":
                    case "BINARY":
                        ret.SystemType = typeof(byte[]);
                        break;
                    case "TIME":
                        ret.SystemType = typeof(TimeSpan?);
                        break;
                    case "BIT":
                        ret.SystemType = typeof(bool?);
                        break;
                    default:
                        throw new ArgumentException(ExceptionMessages.InvalidSqlDataType, nameof(column));
                }
            }
            catch (ArgumentException)
            {
                ret.SystemType = typeof(object);
            }

            if (column.PKIsIdentity && Nullable.GetUnderlyingType(ret.SystemType) != null)
                ret.SystemType = Nullable.GetUnderlyingType(ret.SystemType);

            return ret;
        }
    }
}
