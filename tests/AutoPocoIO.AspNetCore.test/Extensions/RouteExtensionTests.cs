using AutoPocoIO.Extensions;
using AutoPocoIO.LoggingMiddleware;
using Microsoft.AspNetCore.Http;
using Xunit;
using Moq;

namespace AutoPocoIO.AspNetCore.test.Extensions
{
    
    [Trait("Category", TestCategories.Unit)]
    public class RouteExtensionTests
    {
        Mock<HttpResponse> response;
        ContextLogParameters logParameters;

        public RouteExtensionTests()
        {
            response = new Mock<HttpResponse>();
            
            var context = new Mock<HttpContext>();
            context.Setup(c => c.Response).Returns(response.Object);
            logParameters = new ContextLogParameters()
            {
                Context = context.Object
            };

        }
        [FactWithName]
        public void GetStatusCode200()
        {
            response.Setup(c => c.StatusCode).Returns(200);
            Assert.Equal("200 : OK", logParameters.DescriptionFromStatusCode(""));
        }

        [FactWithName]
        public void GetStatusCodeCustomCode()
        {
            response.Setup(c => c.StatusCode).Returns(401);
            Assert.Equal("401 : OtherPhrase", logParameters.DescriptionFromStatusCode("OtherPhrase"));
        }
    }
}
