using AutoPocoIO.CustomAttributes;
using AutoPocoIO.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutoPocoIO.test.CustomAttributes
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class DynamicRoutePrefixAttributeTest
    {
        [TestMethod]
        public void DynamicRouteWithPrefix()
        {
            AutoPocoConfiguration.DashboardPathPrefix = "testPrefix";
            var attr = new DynamicRoutePrefixAttribute("test");
            Assert.AreEqual("testPrefix/test", attr.Prefix);
        }

        [TestMethod]
        public void DynamicRouteWithoutPrefix()
        {
            var attr = new DynamicRoutePrefixAttribute("test");
            Assert.AreEqual("test", attr.Prefix);
        }

        [TestCleanup]
        public void Cleanup()
        {
            AutoPocoConfiguration.DashboardPathPrefix = null;
        }
    }
}
