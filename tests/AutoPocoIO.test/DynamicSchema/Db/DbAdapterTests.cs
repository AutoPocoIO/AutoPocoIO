using AutoPocoIO.DynamicSchema.Db;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.DynamicSchema.Runtime;
using AutoPocoIO.Exceptions;
using AutoPocoIO.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoPocoIO.test.DynamicSchema.Db
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public partial class DbAdapterTests
    {
        private Mock<DynamicClassBuilder> classBuilder;
        private Mock<IDbSchemaBuilder> schemaBuilder;
        private Mock<DbSchema> schema;
        private DbAdapter dbAdapter;

        [TestInitialize]
        public void Init()
        {
            schemaBuilder = new Mock<IDbSchemaBuilder>();
            schema = new Mock<DbSchema>();
            schema.Setup(c => c.GetHashCode()).Returns(123456);

            classBuilder = new Mock<DynamicClassBuilder>(schema.Object);
            dbAdapter = new DbAdapter(schemaBuilder.Object, classBuilder.Object, schema.Object);
        }

        private void SetupSchmeaBuilder()
        {
            var tables = new List<Table>() { new Table { Name = "tbl1" } };
            var views = new List<View>() { new View { Name = "view1" } };

            //sequence skips on model building
            schema.SetupSequence(c => c.Tables).Returns(tables).Returns(tables).Returns(new List<Table>());
            schema.SetupGet(c => c.Views).Returns(views);

            var appDbOptions = new DbContextOptionsBuilder()
               .UseInMemoryDatabase(databaseName: "appDb" + Guid.NewGuid().ToString())
               .ReplaceEFCachingServices()
               .Options;

            schemaBuilder.Setup(c => c.CreateDbContextOptions()).Returns(appDbOptions);
        }


        [TestMethod]
        public void SetupDbContextSetsDbInfoTables()
        {
            SetupSchmeaBuilder();
            string asmName = $"DYNAMICASSEMBLY._DBO_TBL1{schema.Object.Tables.First().GetHashCode()}._DBO_TBL1123456";
            var types = new Dictionary<string, Type>() { { asmName, typeof(Connector) } };
            classBuilder.Setup(c => c.CreateModelTypes("_dbo_tbl1"));
            classBuilder.SetupGet(c => c.ExistingAssemblies).Returns(types);

            dbAdapter.SetupDataContext("_dbo_tbl1");

            Assert.AreEqual(typeof(Connector), dbAdapter.DbSetEntityType);
            Assert.IsInstanceOfType(dbAdapter.GetDbSet(), typeof(DbSet<Connector>));
        }



        [TestMethod]
        public void SetupDbContextSetsDbInfoViews()
        {
            SetupSchmeaBuilder();
            var tables = new List<Table>() { new Table { Name = "tbl1" } };
            schema.SetupSequence(c => c.Tables).Returns(tables).Returns(new List<Table>());

            string asmName = $"DYNAMICASSEMBLY._DBO_VIEW1{schema.Object.Views.First().GetHashCode()}._DBO_VIEW1123456";
            var types = new Dictionary<string, Type>() { { asmName, typeof(Connector) } };
            classBuilder.Setup(c => c.CreateModelTypes("_dbo_view1"));
            classBuilder.SetupGet(c => c.ExistingAssemblies).Returns(types);

            dbAdapter.SetupDataContext("_dbo_view1");

            Assert.AreEqual(typeof(Connector), dbAdapter.DbSetEntityType);
            Assert.IsInstanceOfType(dbAdapter.GetDbSet(), typeof(DbSet<Connector>));
        }

        [TestMethod]
        public void DisposeClearsValues()
        {
            SetupSchmeaBuilder();
            var tables = new List<Table>() { new Table { Name = "tbl1" } };
            schema.SetupSequence(c => c.Tables)
                .Returns(tables) //find table
                .Returns(new List<Table>()); //send to context

            string asmName = $"DYNAMICASSEMBLY._DBO_TBL1{tables.First().GetHashCode()}._DBO_TBL1123456";
            var types = new Dictionary<string, Type>() { { asmName, typeof(Connector) } };
            classBuilder.Setup(c => c.CreateModelTypes("_dbo_tbl1"));
            classBuilder.SetupGet(c => c.ExistingAssemblies).Returns(types);

            dbAdapter.SetupDataContext("_dbo_tbl1");

            Assert.IsNotNull(dbAdapter.Instance);

            dbAdapter.Dispose();

            Assert.IsNull(dbAdapter.Instance);
        }

        private void SetupForGetWithoutContext(string asmName)
        {
            var tables = new List<Table>() { new Table { Name = "tbl1" } };
            var views = new List<View>() { new View { Name = "view1" } };
            
            schema.SetupGet(c => c.Tables).Returns(tables);
            schema.SetupGet(c => c.Views).Returns(views);

            var appDbOptions = new DbContextOptionsBuilder()
                 .UseInMemoryDatabase(databaseName: "appDb" + Guid.NewGuid().ToString())
                 .Options;

            var types = new Dictionary<string, Type>() { { asmName, typeof(Connector) } };
            classBuilder.SetupGet(c => c.ExistingAssemblies).Returns(types);

            var dbAdapterMock = new Mock<DbAdapter>(schemaBuilder.Object, classBuilder.Object, schema.Object);

            var instance = new DbContextBase(appDbOptions, types, new List<Table>());
            dbAdapterMock.SetupGet(c => c.Instance).Returns(instance);

            dbAdapter = dbAdapterMock.Object;

        }

        [TestMethod]
        public void GetWithoutContextTables()
        {
            var hashCode = new Table { Name = "tbl1" }.GetHashCode();
            string asmName = $"DYNAMICASSEMBLY._DBO_TBL1{hashCode}.OUTER1123456";
            SetupForGetWithoutContext(asmName);
            var result = dbAdapter.GetWithoutContext("_dbo_tbl1", "outer1");
            Assert.IsInstanceOfType(result, typeof(DbSet<Connector>));
        }

        [TestMethod]
        public void GetWithoutContextViews()
        {
            var hashCode = new View { Name = "view1" }.GetHashCode();
            string asmName = $"DYNAMICASSEMBLY._DBO_VIEW1{hashCode}.OUTER1123456";
            SetupForGetWithoutContext(asmName);
            var result = dbAdapter.GetWithoutContext("_dbo_view1", "outer1");
            Assert.IsInstanceOfType(result, typeof(DbSet<Connector>));
        }

        [TestMethod]
        public void MapPrimaryKeysFromObject()
        {
            SetupSchmeaBuilder();
            var table = new Table { Name = "tbla" };
            table.Columns.AddRange(new[] 
            {
                new Column { ColumnName = "Id", PKName = "pk1" },
                new Column { ColumnName = "Name" }
            });

            schema.Setup(c => c.Tables).Returns(new List<Table>() { table });
            string asmName = $"DYNAMICASSEMBLY._DBO_TBLA{schema.Object.Tables.First().GetHashCode()}._DBO_TBLA123456";
            var types = new Dictionary<string, Type>() { { asmName, typeof(_dbo_tbla) } };
            classBuilder.Setup(c => c.CreateModelTypes("_dbo_tbla"));
            classBuilder.SetupGet(c => c.ExistingAssemblies).Returns(types);

            dbAdapter.SetupDataContext("_dbo_tbla");

            var model = new { id = 1, name = "test" };
            var keys = dbAdapter.MapPrimaryKey(model);

            Assert.AreEqual(1, keys.Count());
            Assert.AreEqual("Id", keys.First().Name);
            Assert.AreEqual(typeof(int), keys.First().Type);
            Assert.AreEqual(1, keys.First().Value);
        }

        [TestMethod]
        public void MapPrimaryKeysFromObjectCompoundKey()
        {
            SetupSchmeaBuilder();
            var table = new Table { Name = "tblb" };
            table.Columns.AddRange(new[]
            {
                new Column { ColumnName = "Id", PKName = "pk1", PKPosition = 2 },
                new Column { ColumnName = "Name", PKName = "pk2", PKPosition = 1 }
            });

            schema.Setup(c => c.Tables).Returns(new List<Table>() { table });
            string asmName = $"DYNAMICASSEMBLY._DBO_TBLB{schema.Object.Tables.First().GetHashCode()}._DBO_TBLB123456";
            var types = new Dictionary<string, Type>() { { asmName, typeof(_dbo_tblb) } };
            classBuilder.Setup(c => c.CreateModelTypes("_dbo_tblb"));
            classBuilder.SetupGet(c => c.ExistingAssemblies).Returns(types);

            dbAdapter.SetupDataContext("_dbo_tblb");

            var model = new { id = 1, name = "test" };
            var keys = dbAdapter.MapPrimaryKey(model);

            Assert.AreEqual(2, keys.Count());
            Assert.AreEqual("Name", keys.First().Name);
            Assert.AreEqual(typeof(string), keys.First().Type);
            Assert.AreEqual("test", keys.First().Value);
            Assert.AreEqual("Id", keys.Last().Name);
            Assert.AreEqual(typeof(int), keys.Last().Type);
            Assert.AreEqual(1, keys.Last().Value);
        }

        [TestMethod]
        [ExpectedException(typeof(PrimaryKeyNotFoundException))]
        public void MapPrimaryKeysFromObjectKeyNotFound()
        {
            SetupSchmeaBuilder();
            var table = new Table { Name = "tblc" };
            table.Columns.AddRange(new[]
            {
                new Column { ColumnName = "Id", PKName = "pk1" },
                new Column { ColumnName = "Name" }
            });

            schema.Setup(c => c.Tables).Returns(new List<Table>() { table });
            string asmName = $"DYNAMICASSEMBLY._DBO_TBLC{schema.Object.Tables.First().GetHashCode()}._DBO_TBLC123456";
            var types = new Dictionary<string, Type>() { { asmName, typeof(_dbo_tblc) } };
            classBuilder.Setup(c => c.CreateModelTypes("_dbo_tblc"));
            classBuilder.SetupGet(c => c.ExistingAssemblies).Returns(types);

            dbAdapter.SetupDataContext("_dbo_tblc");

            var model = new { name = "test" };
            dbAdapter.MapPrimaryKey(model);
        }


    }
}
