using AutoPocoIO.Models;
using Xunit;

namespace AutoPocoIO.test.Migrations
{
    [Trait("Category", TestCategories.Unit)]
    public class UserJoinTests
    {
        //Exists so migration creation makes FKs
        [FactWithName]
        public void BasicUserJoinHasNavProperties()
        {
            var conn1 = new Connector();
            var conn2 = new Connector();

            UserJoin userJoin = new UserJoin
            {
                PKConnector = conn1,
                FKConnector = conn2
            };

            Assert.Equal(conn1, userJoin.PKConnector);
            Assert.Equal(conn2, userJoin.FKConnector);
        }
    }
}
