using AutoPocoIO.DynamicSchema.Db;
using AutoPocoIO.DynamicSchema.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Linq;

namespace AutoPocoIO.test.DynamicSchema.Db
{
    [Trait("Category", TestCategories.Unit)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public class ContextBaseTests
    {
        private readonly DbContextOptions options;
        public ContextBaseTests()
        {
            options = new DbContextOptionsBuilder<DbContextBase>()
              .UseInMemoryDatabase(databaseName: "appDb" + Guid.NewGuid().ToString())
              .Options;
        }


        private class Class1
        {
            [Key]
            public int Prop1 { get; set; }
        }

        private class _dbo_ClassNoKeyAttr
        {
            public int Prop1 { get; set; }
        }


        private class _dbo_ClassNoKeyAttr2Props
        {
            public int Prop1 { get; set; }
            public int Prop2 { get; set; }
        }

        private class Ventity__dbo_veClass
        {
            public int Prop1 { get; set; }
            public int Prop2 { get; set; }
            public _dbo_ClassNoKeyAttr2Props VEToBase_ClassNoKeyAttr2PropsObject { get; set; }
        }

        private class Ventity__dbo_veClass2
        {
            public int Prop1 { get; set; }
            public int Prop2 { get; set; }
            public _dbo_ClassNoKeyAttr VEToBase_ClassNoKeyAttrObject { get; set; }
        }

        private class Ventity__dbo_veClassRefA
        {
            public int Prop1 { get; set; }
            public _dbo_ClassNoKeyAttr2Props VEToBase_ClassNoKeyAttr2PropsObject { get; set; }
            public Ventity__dbo_veClassRefB VEToVE_veClassRefBObject { get; set; }
        }

        private class Ventity__dbo_veClassRefB
        {
            public int Prop1 { get; set; }
            public _dbo_ClassNoKeyAttr2Props VEToBase_ClassNoKeyAttr2PropsObject { get; set; }
            public Ventity__dbo_veClassRefA VEToVE_veClassRefAObject { get; set; }
        }

        private class Ventity__dbo_veMissingLink
        {
            public int Prop1 { get; set; }
        }

        #region contexts
        //NewInstances to force OnModelCreate to be unique
        private class TestableDbContextBase : DbContextBase
        {
            internal TestableDbContextBase(DbContextOptions options, Dictionary<string, Type> assemblyTypes, List<Table> tables)
                : base(options, assemblyTypes, tables)
            { }
            public void TestModelCreation(ModelBuilder model) { OnModelCreating(model); }
        }

        private class TestableDbContextBase1 : DbContextBase
        {
            internal TestableDbContextBase1(DbContextOptions options, Dictionary<string, Type> assemblyTypes, List<Table> tables)
               : base(options, assemblyTypes, tables)
            { }
            public void TestModelCreation(ModelBuilder model) { OnModelCreating(model); }
        }

        private class TestableDbContextBase2 : DbContextBase
        {
            internal TestableDbContextBase2(DbContextOptions options, Dictionary<string, Type> assemblyTypes, List<Table> tables)
                : base(options, assemblyTypes, tables)
            { }
            public void TestModelCreation(ModelBuilder model) { OnModelCreating(model); }
        }
        #endregion

        [FactWithName]
        public void CheckSetExists()
        {

            var asms = new Dictionary<string, Type> { { "conn", typeof(Class1) } };
            var context = new TestableDbContextBase(options, asms, new List<Table>());

            context.TestModelCreation(new ModelBuilder(new ConventionSet()));

            Assert.Single(context.Model.GetEntityTypes());
            Assert.Equal(typeof(Class1), context.Model.GetEntityTypes().First().ClrType);
        }

        [FactWithName]
        public void AddPksFromTableList()
        {
            var asms = new Dictionary<string, Type> { { "conn", typeof(_dbo_ClassNoKeyAttr) } };
            var tableList = new List<Table> { new Table { Name = "ClassNoKeyAttr" } };
            tableList[0].Columns.Add(new Column { ColumnName = "Prop1", PKName = "pk" });


            var context = new TestableDbContextBase1(options, asms, tableList);
            context.TestModelCreation(new ModelBuilder(new ConventionSet()));

            Assert.Single(context.Model.GetEntityTypes());
            Assert.Equal(typeof(_dbo_ClassNoKeyAttr), context.Model.GetEntityTypes().First().ClrType);

            string keyName = context.Model.GetEntityTypes().First().FindPrimaryKey().Properties.Select(x => x.Name).Single();
            Assert.Equal("Prop1", keyName);
        }

        [FactWithName]
        public void AddPksMultipleFromTableList()
        {
            var asms = new Dictionary<string, Type> { { "conn", typeof(_dbo_ClassNoKeyAttr2Props) } };
            var tableList = new List<Table> { new Table { Name = "ClassNoKeyAttr2Props" } };
            tableList[0].Columns.AddRange(new List<Column> {
                    new Column { ColumnName = "Prop1", PKName = "pk" },
                    new Column { ColumnName = "Prop2", PKName = "pk" }
            });

            var context = new TestableDbContextBase2(options, asms, tableList);
            context.TestModelCreation(new ModelBuilder(new ConventionSet()));

            Assert.Single(context.Model.GetEntityTypes());
            Assert.Equal(typeof(_dbo_ClassNoKeyAttr2Props), context.Model.GetEntityTypes().First().ClrType);

            var keyName = context.Model.GetEntityTypes().First().FindPrimaryKey().Properties.Select(x => x.Name);
            Assert.Equal(2, keyName.Count());
            Assert.Equal("Prop1", keyName.First());
            Assert.Equal("Prop2", keyName.Last());
        }

        [FactWithName]
        public void CrateCommandFromDatabaseFacade()
        {
            var dbCommand = new Mock<DbCommand>();

            var connection = new Mock<DbConnection>();
            connection.Protected().Setup<DbCommand>("CreateDbCommand")
                .Returns(dbCommand.Object);

            var connService = new Mock<IRelationalConnection>();
            connService.Setup(c => c.DbConnection).Returns(connection.Object);

            var services = new ServiceCollection();
            services.AddEntityFrameworkInMemoryDatabase();
            services.AddSingleton(connService.Object);

            var options = new DbContextOptionsBuilder<DbContextBase>()
              .UseInMemoryDatabase(databaseName: "appDb" + Guid.NewGuid().ToString())
              .UseInternalServiceProvider(services.BuildServiceProvider())
              .Options;


            var context = new Mock<DbContextBase>(options, new Dictionary<string, Type>(), new List<Table>());
            var dbFacade = new Mock<DatabaseFacade>(context.Object);
            context.Setup(c => c.Database).Returns(dbFacade.Object);
            context.CallBase = true;

            var result = context.Object.CreateDbCommand();

            Assert.Equal(dbCommand.Object, result);
        }
    }
}
