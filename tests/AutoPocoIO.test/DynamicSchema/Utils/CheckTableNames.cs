using AutoPocoIO.DynamicSchema.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutoPocoIO.test.DynamicSchema.Utils
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class CheckTableNames
    {
        [TestMethod]
        public void ConnectorsTableName()
        {
            Assert.AreEqual("Connector", DefaultTables.Connectors);
        }

        [TestMethod]
        public void RequestLogTableName()
        {
            Assert.AreEqual("Request", DefaultTables.RequestLogs);
        }

        [TestMethod]
        public void ResponseLogTableName()
        {
            Assert.AreEqual("Response", DefaultTables.ResponseLogs);
        }

        [TestMethod]
        public void UserJoinTableName()
        {
            Assert.AreEqual("UserJoin", DefaultTables.UserJoins);
        }
    }
}
