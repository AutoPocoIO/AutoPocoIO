using AutoPocoIO.CustomAttributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace AutoPocoIO.AspNetCore.test.CustomAttributes
{
    [TestClass]
    public class UseJsonTest
    {
        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void VerifyProduces()
        {
            var attr = new UseJsonAttribute();

            Assert.AreEqual(2, attr.ContentTypes.Count);
            Assert.AreEqual("application/json", attr.ContentTypes.First());
            Assert.AreEqual("text/plain", attr.ContentTypes.Last());
        }
    }
}
