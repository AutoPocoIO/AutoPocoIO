using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.DynamicSchema.Util;
using AutoPocoIO.MsSql.DynamicSchema.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetTopologySuite.Geometries;
using System;

namespace AutoPocoIO.test.DynamicSchema.Db
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class MsSqlTypeMapper
    {
        private IDbTypeMapper mapper;

        [TestInitialize]
        public void Init()
        {
            mapper = new MsSqlDbTypeMapper();
        }

        [TestMethod]
        public void UniqueIdentifierToGuidCallsBase()
        {
            var column = new Column { ColumnType = "uniqueIdentifier" };
            var result = mapper.DBTypeToDataType(column);

            Assert.AreEqual("uniqueIdentifier", result.DbType);
            Assert.AreEqual(typeof(Guid?), result.SystemType);
        }


        [TestMethod]
        public void GeographyToGeometry()
        {
            var column = new Column { ColumnType = "geography" };
            var result = mapper.DBTypeToDataType(column);

            Assert.AreEqual("geography", result.DbType);
            Assert.AreEqual(typeof(Geometry), result.SystemType);
        }


        [TestMethod]
        public void GeometryToGeometry()
        {
            var column = new Column { ColumnType = "geometry" };
            var result = mapper.DBTypeToDataType(column);

            Assert.AreEqual("geometry", result.DbType);
            Assert.AreEqual(typeof(Geometry), result.SystemType);
        }

        [TestMethod]
        public void GeometryToNullGeometry()
        {
            var column = new Column { ColumnType = "geometry", ColumnIsNullable = true };
            var result = mapper.DBTypeToDataType(column);

            Assert.AreEqual("geometry(nullable)", result.DbType);
            Assert.AreEqual(typeof(Geometry), result.SystemType);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void HierarchyIdNotSupported()
        {
            var column = new Column { ColumnType = "hierarchyid" };
            mapper.DBTypeToDataType(column);
        }
    }
}
