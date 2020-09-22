using AutoPocoIO.DynamicSchema.Db;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.Exceptions;
using AutoPocoIO.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutoPocoIO.test.Extensions
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class DynamicSchemaViewExtensionTests
    {
        private readonly DbSchema dbSchema = new DbSchema();

        [TestMethod]
        public void FindView()
        {
            dbSchema.Views.Add(new View { Name = "name1", Schema = "sch1", Database = "db" });
            dbSchema.Views.Add(new View { Name = "name2", Schema = "sch1", Database = "db" });

            View tbl = dbSchema.GetView("sch1", "name2");
            Assert.AreEqual("name2", tbl.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(ViewNotFoundException))]
        public void FindViewThrowException()
        {
            dbSchema.Views.Add(new View { Name = "name1", Schema = "sch1", Database = "db" });
            dbSchema.Views.Add(new View { Name = "name2", Schema = "sch1", Database = "db" });
            _ = dbSchema.GetView("sch1", "name21");
        }
    }
}
