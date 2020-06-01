using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.DynamicSchema.Util;
using AutoPocoIO.MsSql.DynamicSchema.Utils;
using Xunit;
using NetTopologySuite.Geometries;
using System;

namespace AutoPocoIO.MsSql.test.DynamicSchema.Db
{
    
    [Trait("Category", TestCategories.Unit)]
    public class MsSqlTypeMapper
    {
        private IDbTypeMapper mapper;

        public MsSqlTypeMapper()
        {
            mapper = new MsSqlDbTypeMapper();
        }

        [FactWithName]
        public void UniqueIdentifierToGuidCallsBase()
        {
            var column = new Column { ColumnType = "uniqueIdentifier" };
            var result = mapper.DBTypeToDataType(column);

            Assert.Equal("uniqueIdentifier", result.DbType);
            Assert.Equal(typeof(Guid?), result.SystemType);
        }


        [FactWithName]
        public void GeographyToGeometry()
        {
            var column = new Column { ColumnType = "geography" };
            var result = mapper.DBTypeToDataType(column);

            Assert.Equal("geography", result.DbType);
            Assert.Equal(typeof(Geometry), result.SystemType);
        }


        [FactWithName]
        public void GeometryToGeometry()
        {
            var column = new Column { ColumnType = "geometry" };
            var result = mapper.DBTypeToDataType(column);

            Assert.Equal("geometry", result.DbType);
            Assert.Equal(typeof(Geometry), result.SystemType);
        }

        [FactWithName]
        public void GeometryToNullGeometry()
        {
            var column = new Column { ColumnType = "geometry", ColumnIsNullable = true };
            var result = mapper.DBTypeToDataType(column);

            Assert.Equal("geometry(nullable)", result.DbType);
            Assert.Equal(typeof(Geometry), result.SystemType);
        }

        [FactWithName]
        public void HierarchyIdNotSupported()
        {
            var column = new Column { ColumnType = "hierarchyid" };
            void act() => mapper.DBTypeToDataType(column);
            Assert.Throws<ArgumentException>(act);
        }
    }
}
