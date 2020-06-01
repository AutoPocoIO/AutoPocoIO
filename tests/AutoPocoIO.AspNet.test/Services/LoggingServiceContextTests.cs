using AutoPocoIO.LoggingMiddleware;
using AutoPocoIO.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace AutoPocoIO.AspNet.test
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class LoggingServiceContextTests
    {
        ITimeProvider timeProvider;
        [TestInitialize]
        public void Init()
        {
            var mock = new Mock<ITimeProvider>();
            mock.Setup(c => c.UtcNow).Returns(new DateTime(2011, 1, 1));
            timeProvider = mock.Object;
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CheckForLogParameters()
        {
            LoggingService service = new LoggingService(timeProvider, Mock.Of<IServiceScopeFactory>());
            service.AddContextInfomation(null);
        }

        [TestMethod]
        public void SetContextInformation()
        {
            var request = new Mock<IOwinRequest>();
            request.Setup(c => c.RemoteIpAddress).Returns("127.0.0.2");

            var response = new Mock<IOwinResponse>();
            response.Setup(c => c.StatusCode).Returns(200);
            response.Setup(c => c.ReasonPhrase).Returns("OK");

            var context = new Mock<IOwinContext>();
            context.Setup(c => c.Request).Returns(request.Object);
            context.Setup(c => c.Response).Returns(response.Object);

            ContextLogParameters logParameters = new ContextLogParameters()
            {
                Context = context.Object,
                Exception = "ex123"
            };

            LoggingService service = new LoggingService(timeProvider, Mock.Of<IServiceScopeFactory>());
            service.AddContextInfomation(logParameters);


            Assert.AreEqual(new DateTime(2011, 1, 1), service.ResponseTime);
            Assert.AreEqual("200 : OK", service.StatusCode);
            Assert.AreEqual("127.0.0.2", service.Ip);
            Assert.AreEqual("ex123", service.Exception);
        }
    }
}
