using AutoPocoIO.Extensions;
using Xunit;
using System.Collections.Generic;
using System.Web;

namespace AutoPocoIO.AspNet.test.Extensions
{
    
    [Trait("Category", TestCategories.Unit)]
    public class MvcRouteExtensions
    {
        [FactWithName]
        public void NullRequestReturnsNewDictionary()
        {
            HttpRequest req = null;
            var results = req.GetQueryStrings();
            Assert.Equal(new Dictionary<string, string>(), results);
        }

        [FactWithName]
        public void MvcRequestReturnsQueryStringIgnoreCase()
        {
            HttpRequest req = new HttpRequest("", "http://test.com", "abc=123");
            var results = req.GetQueryStrings();
            Assert.Equal("123", results["abc"]);
            Assert.Equal("123", results["ABC"]);
        }

        [FactWithName]
        public void MvcRequestReturnsNoQueryString()
        {
            HttpRequest req = new HttpRequest("", "http://test.com", null);
            var results = req.GetQueryStrings();
            Assert.Equal(new Dictionary<string, string>(), results);
        }
    }
}
