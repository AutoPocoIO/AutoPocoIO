using AutoPocoIO.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Web;

namespace AutoPocoIO.AspNet.test.Extensions
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class MvcRouteExtensions
    {
        [TestMethod]
        public void NullRequestReturnsNewDictionary()
        {
            HttpRequest req = null;
            var results = req.GetQueryStrings();
            CollectionAssert.AreEqual(new Dictionary<string, string>(), results);
        }

        [TestMethod]
        public void MvcRequestReturnsQueryStringIgnoreCase()
        {
            HttpRequest req = new HttpRequest("", "http://test.com", "abc=123");
            var results = req.GetQueryStrings();
            Assert.AreEqual("123", results["abc"]);
            Assert.AreEqual("123", results["ABC"]);
        }

        [TestMethod]
        public void MvcRequestReturnsNoQueryString()
        {
            HttpRequest req = new HttpRequest("", "http://test.com", null);
            var results = req.GetQueryStrings();
            CollectionAssert.AreEqual(new Dictionary<string, string>(), results);
        }
    }
}
