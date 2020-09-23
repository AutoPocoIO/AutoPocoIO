using AutoPocoIO.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutoPocoIO.test.Migrations
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class UserJoinTests
    {
        //Exists so migration creation makes FKs
        [TestMethod]
        public void BasicUserJoinHasNavProperties()
        {
            var conn1 = new Connector();
            var conn2 = new Connector();

            UserJoin userJoin = new UserJoin
            {
                PKConnector = conn1,
                FKConnector = conn2
            };

            Assert.AreEqual(conn1, userJoin.PKConnector);
            Assert.AreEqual(conn2, userJoin.FKConnector);
        }
    }
}
