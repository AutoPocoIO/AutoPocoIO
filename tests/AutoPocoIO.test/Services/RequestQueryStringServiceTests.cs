using AutoPocoIO.Services;
using Microsoft.Extensions.Primitives;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace AutoPocoIO.test.Services
{
    [Trait("Category", TestCategories.Unit)]
    public class RequestQueryStringServiceTests
    {
        [FactWithName]
        public void GetQueryStringsFromContext()
        {
#if NETFULL
            var textWriter = new Mock<TextWriter>();
            var request = new System.Web.HttpRequest("", "http://test.com", "prop1=abc&prop2=123");
            var context = new System.Web.HttpContext(request, new System.Web.HttpResponse(textWriter.Object));

            System.Web.HttpContext.Current = context;
            var service = new RequestQueryStringService();
#else
            var collection = new Dictionary<string, StringValues>() { { "prop1", "abc" }, { "prop2", "123" } };
            var query = new Microsoft.AspNetCore.Http.Internal.QueryCollection(collection);
            var req = new Mock<Microsoft.AspNetCore.Http.HttpRequest>();
            req.Setup(c => c.Query).Returns(query);

            var context = new Mock<Microsoft.AspNetCore.Http.HttpContext>();
            context.Setup(c => c.Request).Returns(req.Object);

            var contextAccess = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
            contextAccess.Setup(c => c.HttpContext).Returns(context.Object);

            var service = new RequestQueryStringService(contextAccess.Object);
#endif


            var results = service.GetQueryStrings();
            var expected = new Dictionary<string, string>() { { "prop1", "abc" }, { "prop2", "123" } };
            Assert.Equal(expected, results.ToDictionary(c => c.Key, c => c.Value));
        }
    }
}
