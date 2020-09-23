using AutoPocoIO.DynamicSchema.Db;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.Exceptions;
using AutoPocoIO.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutoPocoIO.test.Extensions
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class DynamicSchemaSprocExtensionTests
    {
        private DbSchema dbSchema;

        [TestInitialize]
        public void Init()
        {
            dbSchema = new DbSchema();
        }

        [TestMethod]
        public void FindProc()
        {
            dbSchema.StoredProcedures.Add(new StoredProcedure { Name = "name1", Schema = "sch1" });
            dbSchema.StoredProcedures.Add(new StoredProcedure { Name = "name2", Schema = "sch1" });

            var tbl = dbSchema.GetStoredProcedure("sch1", "name2");
            Assert.AreEqual("name2", tbl.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(StoreProcedureNotFoundException))]
        public void FindProchrowException()
        {
            dbSchema.StoredProcedures.Add(new StoredProcedure { Name = "name1", Schema = "sch1" });
            dbSchema.StoredProcedures.Add(new StoredProcedure { Name = "name2", Schema = "sch1" });
            _ = dbSchema.GetStoredProcedure("sch1", "name12");
        }

        [TestMethod]
        [ExpectedException(typeof(StoreProcedureNotFoundException))]
        public void FindProcDifferentSchemaThrowException()
        {
            dbSchema.StoredProcedures.Add(new StoredProcedure { Name = "name1", Schema = "sch1" });
            dbSchema.StoredProcedures.Add(new StoredProcedure { Name = "name2", Schema = "sch1" });
            _ = dbSchema.GetStoredProcedure("sch2", "name1");
        }
    }
}
