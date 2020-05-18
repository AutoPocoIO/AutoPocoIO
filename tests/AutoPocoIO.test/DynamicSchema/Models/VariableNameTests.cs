using AutoPocoIO.DynamicSchema.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutoPocoIO.test.DynamicSchema.Models
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class VariableNameTests
    {
        [TestMethod]
        public void TableStringIsVariableName()
        {
            var tbl = new Table { Database = "db1", Schema = "sch1", Name = "tbl1" };

            Assert.AreEqual("db1_sch1_tbl1", tbl.ToString());
            Assert.AreEqual(tbl.VariableName, tbl.ToString());
        }
    }
}
