using AutoPocoIO.Extensions;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace AutoPocoIO.test.Extensions
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class RouteExtensionTests
    {
#if NETFULL
        System.Net.Http.HttpRequestMessage request;
#else
        Microsoft.AspNetCore.Http.HttpRequest request;
#endif

        [TestInitialize]
        public void Init()
        {
#if NETFULL
            request = new System.Net.Http.HttpRequestMessage
            {
                RequestUri = new Uri("http://test.com?prop1=abc&prop2=123")
            };
#else
            var collection = new Dictionary<string, StringValues>() { { "prop1", "abc" }, { "prop2", "123" } };

            var query = new Microsoft.AspNetCore.Http.Internal.QueryCollection(collection);

            var mock = new Mock<Microsoft.AspNetCore.Http.HttpRequest>();
            mock.Setup(c => c.Query).Returns(query);
            request = mock.Object;
#endif
        }

        [TestMethod]
        public void GetQueryStringAsDictionary()
        {
            var results = request.GetQueryStrings();
            var expected = new Dictionary<string, string>() { { "prop1", "abc" }, { "prop2", "123" } };

            CollectionAssert.AreEqual(expected, results);
        }

        [TestMethod]
        public void NullRequestQueryStringAsDictionary()
        {
            request = null;
            var results = request.GetQueryStrings();
            var expected = new Dictionary<string, string>();

            CollectionAssert.AreEqual(expected, results);
        }

       

    }
}
