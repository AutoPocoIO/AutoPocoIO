using AutoPocoIO.Extensions;
using Microsoft.Extensions.Primitives;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace AutoPocoIO.test.Extensions
{

    [Trait("Category", TestCategories.Unit)]
    public class RouteExtensionTests
    {
#if NETFULL
        System.Net.Http.HttpRequestMessage request;
#else
        Microsoft.AspNetCore.Http.HttpRequest request;
#endif

        public RouteExtensionTests()
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

        [FactWithName]
        public void GetQueryStringAsDictionary()
        {
            var results = request.GetQueryStrings();
            var expected = new Dictionary<string, string>() { { "prop1", "abc" }, { "prop2", "123" } };

            Assert.Equal(expected, results);
        }

        [FactWithName]
        public void NullRequestQueryStringAsDictionary()
        {
            request = null;
            var results = request.GetQueryStrings();
            var expected = new Dictionary<string, string>();

            Assert.Equal(expected, results);
        }

       

    }
}
