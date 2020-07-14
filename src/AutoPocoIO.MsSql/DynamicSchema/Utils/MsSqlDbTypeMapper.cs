using AutoPocoIO.Constants;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.DynamicSchema.Util;
using NetTopologySuite.Geometries;
using System;

namespace AutoPocoIO.MsSql.DynamicSchema.Utils
{
    public class MsSqlDbTypeMapper : DbTypeMapper, IDbTypeMapper
    {
        public override DataType DBTypeToDataType(Column column)
        {
            string dataType = column.ColumnType.ToUpperInvariant();
            DataType ret = new DataType
            {
                DbType = column.ColumnType
            };


            if (column.ColumnIsNullable)
            {
                ret.DbType += "(nullable)";
            }

            switch (dataType)
            {
                case "HIERARCHYID":
                    throw new ArgumentException(ExceptionMessages.HierarchyIdNotSupported, nameof(column));
                case "GEOMETRY":
                case "GEOGRAPHY":
                    ret.SystemType = typeof(Geometry);
                    break;
                default:
                    ret = base.DBTypeToDataType(column);
                    break;
            }

            return ret;
        }
    }
}
