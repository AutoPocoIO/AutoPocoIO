using Microsoft.VisualStudio.TestTools.UnitTesting;
using static AutoPocoIO.AutoPocoConstants;

namespace AutoPocoIO.test.DynamicSchema.Utils
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class CheckConnectorNames
    {
        [TestMethod]
        public void AppDbConnectorName()
        {
            Assert.AreEqual("appDb", DefaultConnectors.AppDB);
        }

        [TestMethod]
        public void LogDbConnectorName()
        {
            Assert.AreEqual("logDb", DefaultConnectors.Logging);
        }
    }
}
