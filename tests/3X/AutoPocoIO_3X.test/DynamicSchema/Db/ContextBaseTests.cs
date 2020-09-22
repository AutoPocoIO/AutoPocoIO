using AutoPocoIO.DynamicSchema.Db;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.DynamicSchema.Runtime;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace AutoPocoIO.test.DynamicSchema.Db
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public class ContextBaseTests
    {
        private DbContextOptions options;
        [TestInitialize]
        public void Init()
        {
            options = new DbContextOptionsBuilder<DbContextBase>()
              .UseInMemoryDatabase(databaseName: "appDb" + Guid.NewGuid().ToString())
              .ReplaceEFCachingServices()
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

        private class TestableDbContextBase : DbContextBase
        {
            internal TestableDbContextBase(DbContextOptions options, Dictionary<string, Type> assemblyTypes, List<Table> tables)
                : base(options, assemblyTypes, tables)
            { }
            public void TestModelCreation(ModelBuilder model) { OnModelCreating(model); }
        }



        [TestMethod]
        public void CheckSetExists()
        {

            var asms = new Dictionary<string, Type> { { "conn", typeof(Class1) } };
            var context = new TestableDbContextBase(options, asms, new List<Table>());

            context.TestModelCreation(new ModelBuilder(new ConventionSet()));

            Assert.AreEqual(1, context.Model.GetEntityTypes().Count());
            Assert.AreEqual(typeof(Class1), context.Model.GetEntityTypes().First().ClrType);
        }

        [TestMethod]
        public void AddPksFromTableList()
        {
            var asms = new Dictionary<string, Type> { { "conn", typeof(_dbo_ClassNoKeyAttr) } };
            var tableList = new List<Table> { new Table { Name = "ClassNoKeyAttr" } };
            tableList[0].Columns.Add(new Column { ColumnName = "Prop1", PKName = "pk" });


            var context = new TestableDbContextBase(options, asms, tableList);
            context.TestModelCreation(new ModelBuilder(new ConventionSet()));

            Assert.AreEqual(1, context.Model.GetEntityTypes().Count());
            Assert.AreEqual(typeof(_dbo_ClassNoKeyAttr), context.Model.GetEntityTypes().First().ClrType);

            string keyName = context.Model.GetEntityTypes().First().FindPrimaryKey().Properties.Select(x => x.Name).Single();
            Assert.AreEqual("Prop1", keyName);
        }

        [TestMethod]
        public void AddPksMultipleFromTableList()
        {
            var asms = new Dictionary<string, Type> { { "conn", typeof(_dbo_ClassNoKeyAttr2Props) } };
            var tableList = new List<Table> { new Table { Name = "ClassNoKeyAttr2Props" } };
            tableList[0].Columns.AddRange(new List<Column> {
                    new Column { ColumnName = "Prop1", PKName = "pk" },
                    new Column { ColumnName = "Prop2", PKName = "pk" }
            });

            var context = new TestableDbContextBase(options, asms, tableList);
            context.TestModelCreation(new ModelBuilder(new ConventionSet()));

            Assert.AreEqual(1, context.Model.GetEntityTypes().Count());
            Assert.AreEqual(typeof(_dbo_ClassNoKeyAttr2Props), context.Model.GetEntityTypes().First().ClrType);

            var keyName = context.Model.GetEntityTypes().First().FindPrimaryKey().Properties.Select(x => x.Name);
            Assert.AreEqual(2, keyName.Count());
            Assert.AreEqual("Prop1", keyName.First());
            Assert.AreEqual("Prop2", keyName.Last());
        }
    }
}
