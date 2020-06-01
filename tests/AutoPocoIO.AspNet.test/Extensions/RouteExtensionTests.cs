using AutoPocoIO.Extensions;
using AutoPocoIO.LoggingMiddleware;
using Microsoft.Owin;
using Xunit;
using Moq;

namespace AutoPocoIO.AspNet.test.Extensions
{
    
    [Trait("Category", TestCategories.Unit)]
    public class RouteExtensionTests
    {
        Mock<IOwinResponse> response;
        ContextLogParameters logParameters;

        public RouteExtensionTests()
        {
            response = new Mock<IOwinResponse>();
            var context = new Mock<IOwinContext>();
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
            response.Setup(c => c.ReasonPhrase).Returns("OK");
            Assert.Equal("200 : OK", logParameters.DescriptionFromStatusCode(""));
        }

        [FactWithName]
        public void GetStatusCodeCustomCode()
        {
            response.Setup(c => c.StatusCode).Returns(401);
            response.Setup(c => c.ReasonPhrase).Returns("Unauthorized");
            Assert.Equal("401 : OtherPhrase", logParameters.DescriptionFromStatusCode("OtherPhrase"));
        }
    }
}
