using AutoPocoIO.DynamicSchema.Db;
using AutoPocoIO.DynamicSchema.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutoPocoIO.test.DynamicSchema.Db
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class DbSchemaTests
    {
        [TestMethod]
        public void RequestHashIsValueBased()
        {
            var table = new Table() { Name = "tbl1", Schema = "sch1" };
            var view = new View() { Name = "vw1", Schema = "sch1" };
            var proc = new StoredProcedure() { Name = "proc1", Schema = "sch1" };

            var schema = new DbSchema();
            schema.Tables.Add(table);
            schema.Views.Add(view);
            schema.StoredProcedures.Add(proc);

            var schema2 = new DbSchema();
            schema2.Tables.Add(table);
            schema2.Views.Add(view);
            schema2.StoredProcedures.Add(proc);

            Assert.AreEqual(schema.GetHashCode(), schema2.GetHashCode());
        }
    }
}
