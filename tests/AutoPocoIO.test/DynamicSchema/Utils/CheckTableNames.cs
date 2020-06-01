using AutoPocoIO.DynamicSchema.Util;
using Xunit;

namespace AutoPocoIO.test.DynamicSchema.Utils
{
    [Trait("Category", TestCategories.Unit)]
    public class CheckTableNames
    {
        [FactWithName]
        public void ConnectorsTableName()
        {
            Assert.Equal("Connector", DefaultTables.Connectors);
        }

        [FactWithName]
        public void RequestLogTableName()
        {
            Assert.Equal("Request", DefaultTables.RequestLogs);
        }

        [FactWithName]
        public void ResponseLogTableName()
        {
            Assert.Equal("Response", DefaultTables.ResponseLogs);
        }

        [FactWithName]
        public void UserJoinTableName()
        {
            Assert.Equal("UserJoin", DefaultTables.UserJoins);
        }
    }
}
