using AutoPocoIO.DynamicSchema.Db;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.DynamicSchema.Runtime;
using AutoPocoIO.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoPocoIO.test.DynamicSchema.Db
{
    [Trait("Category", TestCategories.Unit)]
    public class DbAdapterTests
    {
        private readonly Mock<DynamicClassBuilder> classBuilder;
        private readonly Mock<IDbSchemaBuilder> schemaBuilder;
        private readonly Mock<DbSchema> schema;
        private DbAdapter dbAdapter;

        public DbAdapterTests()
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
               .Options;

            schemaBuilder.Setup(c => c.CreateDbContextOptions()).Returns(appDbOptions);
        }


        [FactWithName]
        public void SetupDbContextSetsDbInfoTables()
        {
            SetupSchmeaBuilder();
            string asmName = $"DYNAMICASSEMBLY._DBO_TBL1{schema.Object.Tables.First().GetHashCode()}._DBO_TBL1123456";
            var types = new Dictionary<string, Type>() { { asmName, typeof(Connector) } };
            classBuilder.Setup(c => c.CreateModelTypes("_dbo_tbl1"));
            classBuilder.SetupGet(c => c.ExistingAssemblies).Returns(types);

            dbAdapter.SetupDataContext("_dbo_tbl1");

            Assert.Equal(typeof(Connector), dbAdapter.DbSetEntityType);
            Assert.IsAssignableFrom<DbSet<Connector>>(dbAdapter.GetDbSet());
        }



        [FactWithName]
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

            Assert.Equal(typeof(Connector), dbAdapter.DbSetEntityType);
            Assert.IsAssignableFrom<DbSet<Connector>>(dbAdapter.GetDbSet());
        }

        [FactWithName]
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

             Assert.NotNull(dbAdapter.Instance);

            dbAdapter.Dispose();

             Assert.Null(dbAdapter.Instance);
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

        [FactWithName]
        public void GetWithoutContextTables()
        {
            var hashCode = new Table { Name = "tbl1" }.GetHashCode();
            string asmName = $"DYNAMICASSEMBLY._DBO_TBL1{hashCode}.OUTER1123456";
            SetupForGetWithoutContext(asmName);
            var result = dbAdapter.GetWithoutContext("_dbo_tbl1", "outer1");
            Assert.IsAssignableFrom<DbSet<Connector>>(result);
        }

        [FactWithName]
        public void GetWithoutContextViews()
        {
            var hashCode = new View { Name = "view1" }.GetHashCode();
            string asmName = $"DYNAMICASSEMBLY._DBO_VIEW1{hashCode}.OUTER1123456";
            SetupForGetWithoutContext(asmName);
            var result = dbAdapter.GetWithoutContext("_dbo_view1", "outer1");
            Assert.IsAssignableFrom<DbSet<Connector>>(result);
        }
    }
}
