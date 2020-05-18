using AutoPocoIO.Extensions;
using AutoPocoIO.LoggingMiddleware;
using Microsoft.Owin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AutoPocoIO.AspNet.test.Extensions
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class RouteExtensionTests
    {
        Mock<IOwinResponse> response;
        ContextLogParameters logParameters;

        [TestInitialize]
        public void Init()
        {
            response = new Mock<IOwinResponse>();
            var context = new Mock<IOwinContext>();
            context.Setup(c => c.Response).Returns(response.Object);
            logParameters = new ContextLogParameters()
            {
                Context = context.Object
            };
        }
        [TestMethod]
        public void GetStatusCode200()
        {
            response.Setup(c => c.StatusCode).Returns(200);
            response.Setup(c => c.ReasonPhrase).Returns("OK");
            Assert.AreEqual("200 : OK", logParameters.DescriptionFromStatusCode(""));
        }

        [TestMethod]
        public void GetStatusCodeCustomCode()
        {
            response.Setup(c => c.StatusCode).Returns(401);
            response.Setup(c => c.ReasonPhrase).Returns("Unauthorized");
            Assert.AreEqual("401 : OtherPhrase", logParameters.DescriptionFromStatusCode("OtherPhrase"));
        }
    }
}
