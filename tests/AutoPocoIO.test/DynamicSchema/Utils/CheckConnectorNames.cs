using Xunit;
using static AutoPocoIO.AutoPocoConstants;

namespace AutoPocoIO.test.DynamicSchema.Utils
{
    
     [Trait("Category", TestCategories.Unit)]
    public class CheckConnectorNames
    {
        [FactWithName]
        public void AppDbConnectorName()
        {
            Assert.Equal("appDb", DefaultConnectors.AppDB);
        }

        [FactWithName]
        public void LogDbConnectorName()
        {
            Assert.Equal("logDb", DefaultConnectors.Logging);
        }
    }
}
