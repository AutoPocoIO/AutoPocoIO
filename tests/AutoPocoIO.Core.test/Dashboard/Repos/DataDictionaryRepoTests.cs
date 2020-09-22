using AutoPocoIO.CustomAttributes;
using AutoPocoIO.Dashboard.Repos;
using AutoPocoIO.DynamicSchema.Db;
using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.Factories;
using AutoPocoIO.Models;
using AutoPocoIO.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;

namespace AutoPocoIO.test.Dashboard.Repos
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class DataDictionaryRepoTests
    {
        [TestMethod]
        public void ListDbObjects()
        {
            var table = new Table();
            var view = new View();
            var sp = new StoredProcedure();

            var schema = new Mock<IDbSchema>();
            schema.Setup(c => c.Tables).Returns(new List<Table> { table });
            schema.Setup(c => c.Views).Returns(new List<View> { view });
            schema.Setup(c => c.StoredProcedures).Returns(new List<StoredProcedure> { sp });


            var resource = new Mock<IOperationResource>();
            resource.Setup(c => c.Connector).Returns(new Models.Connector { Name = "test" });
            resource.Setup(c => c.DbSchema).Returns(schema.Object);
            var factory = new Mock<IResourceFactory>();
            factory.Setup(c => c.GetResource("123", "")).Returns(resource.Object);

            var repo = new DataDictionaryRepo(factory.Object);

            var result = repo.ListSchemaObject("123");


            resource.Verify(c => c.LoadSchema(), Times.Once);
            Assert.AreEqual("123", result.ConnectorId);
            Assert.AreEqual("test", result.ConnectorName);
            CollectionAssert.AreEqual(new[] { table }, result.Tables);
            CollectionAssert.AreEqual(new[] { view }, result.Views);
            CollectionAssert.AreEqual(new[] { sp }, result.StoredProcedures);
        }

        [TestMethod]
        public void GetTableDefinition()
        {
            var tableDef = new TableDefinition();
            var resource = new Mock<IOperationResource>();
            resource.Setup(c => c.GetTableDefinition()).Returns(tableDef);

            var factory = new Mock<IResourceFactory>();
            factory.Setup(c => c.GetResourceById("123", OperationType.Any, "tbl12")).Returns(resource.Object);

            var repo = new DataDictionaryRepo(factory.Object);

            var result = repo.ListTableDetails("123", "tbl12");
            Assert.AreEqual(tableDef, result);
        }


        private class OneToManyNav
        {
            public int id { get; set; }
            [ReferencedDbObject("db1", "sch1", "tbl1")]
            public object Nav1 { get; set; }
        }

        [TestMethod]
        public void ListNavPropertyOneToMany()
        {
            var resource = new Mock<IOperationResource>();
            resource.Setup(c => c.GetResourceRecords(new Dictionary<string, string>())).Returns(new List<OneToManyNav>().AsQueryable());

            var factory = new Mock<IResourceFactory>();
            factory.Setup(c => c.GetResourceById("123", OperationType.Any, "tbl12")).Returns(resource.Object);

            var repo = new DataDictionaryRepo(factory.Object);
            var result = repo.ListNavigationProperties("123", "tbl12");

            Assert.AreEqual(1, result.Count());

            Assert.AreEqual("Nav1", result.First().Name);
            Assert.AreEqual("sch1", result.First().ReferencedSchema);
            Assert.AreEqual("tbl1", result.First().ReferencedTable);
            Assert.AreEqual("1 to Many", result.First().Relationship);
        }

        private class ManyToOneNav
        {
            public int id { get; set; }
            [ReferencedDbObject("db1", "sch1", "tbl1")]
            public List<object> Nav1 { get; set; }
        }

        [TestMethod]
        public void ListNavPropertyManyToOne()
        {
            var resource = new Mock<IOperationResource>();
            resource.Setup(c => c.GetResourceRecords(new Dictionary<string, string>())).Returns(new List<ManyToOneNav>().AsQueryable());

            var factory = new Mock<IResourceFactory>();
            factory.Setup(c => c.GetResourceById("123", OperationType.Any, "tbl12")).Returns(resource.Object);

            var repo = new DataDictionaryRepo(factory.Object);
            var result = repo.ListNavigationProperties("123", "tbl12");

            Assert.AreEqual(1, result.Count());

            Assert.AreEqual("Nav1", result.First().Name);
            Assert.AreEqual("sch1", result.First().ReferencedSchema);
            Assert.AreEqual("tbl1", result.First().ReferencedTable);
            Assert.AreEqual("Many to 1", result.First().Relationship);
        }


        private class SkipStrings
        {
            public string id { get; set; }
        }

        [TestMethod]
        public void ExcludeStringProperties()
        {
            var resource = new Mock<IOperationResource>();
            resource.Setup(c => c.GetResourceRecords(new Dictionary<string, string>())).Returns(new List<SkipStrings>().AsQueryable());

            var factory = new Mock<IResourceFactory>();
            factory.Setup(c => c.GetResourceById("123", OperationType.Any, "tbl12")).Returns(resource.Object);

            var repo = new DataDictionaryRepo(factory.Object);
            var result = repo.ListNavigationProperties("123", "tbl12");

            Assert.AreEqual(0, result.Count());
        }

        private class SkipPrivateProperties
        {
            private object Nav { get; set; }
        }

        [TestMethod]
        public void ExcludePrivateProperties()
        {
            var resource = new Mock<IOperationResource>();
            resource.Setup(c => c.GetResourceRecords(new Dictionary<string, string>())).Returns(new List<SkipPrivateProperties>().AsQueryable());

            var factory = new Mock<IResourceFactory>();
            factory.Setup(c => c.GetResourceById("123", OperationType.Any, "tbl12")).Returns(resource.Object);

            var repo = new DataDictionaryRepo(factory.Object);
            var result = repo.ListNavigationProperties("123", "tbl12");

            Assert.AreEqual(0, result.Count());
        }
    }
}
