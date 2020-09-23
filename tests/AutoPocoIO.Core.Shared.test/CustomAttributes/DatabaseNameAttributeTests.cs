using AutoPocoIO.CustomAttributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutoPocoIO.test.CustomAttributes
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class DatabaseNameAttributeTests
    {
        [TestMethod]
        public void ConstructorSetsDatabaseName()
        {
            var attr = new DatabaseNameAttribute("test123");
            Assert.AreEqual("test123", attr.DatabaseName);
        }
    }
}
