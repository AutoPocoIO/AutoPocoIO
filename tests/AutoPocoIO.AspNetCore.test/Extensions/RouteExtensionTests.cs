using AutoPocoIO.Extensions;
using AutoPocoIO.LoggingMiddleware;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AutoPocoIO.AspNetCore.test.Extensions
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class RouteExtensionTests
    {
        Mock<HttpResponse> response;
        ContextLogParameters logParameters;

        [TestInitialize]
        public void Init()
        {
            response = new Mock<HttpResponse>();

            var context = new Mock<HttpContext>();
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
            Assert.AreEqual("200 : OK", logParameters.DescriptionFromStatusCode);
        }

        [TestMethod]
        public void GetStatusCodeCustomCode()
        {
            logParameters.StatusCode = "OtherPhrase";
            response.Setup(c => c.StatusCode).Returns(401);
            Assert.AreEqual("401 : OtherPhrase", logParameters.DescriptionFromStatusCode);
        }
    }
}
