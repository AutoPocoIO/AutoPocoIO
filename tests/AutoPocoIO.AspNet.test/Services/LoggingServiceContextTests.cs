using AutoPocoIO.LoggingMiddleware;
using AutoPocoIO.Services;
using Microsoft.Owin;
using Xunit;
using Moq;
using System;

namespace AutoPocoIO.AspNet.test
{
    
    [Trait("Category", TestCategories.Unit)]
    public class LoggingServiceContextTests
    {
        ITimeProvider timeProvider;
        public LoggingServiceContextTests()
        {
            var mock = new Mock<ITimeProvider>();
            mock.Setup(c => c.UtcNow).Returns(new DateTime(2011, 1, 1));
            timeProvider = mock.Object;
        }

        [FactWithName]
        public void CheckForLogParameters()
        {
            LoggingService service = new LoggingService(timeProvider);
            void act() => service.AddContextInfomation(null);
            Assert.Throws<ArgumentNullException>(act);
        }

        [FactWithName]
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

            LoggingService service = new LoggingService(timeProvider);
            service.AddContextInfomation(logParameters);


            Assert.Equal(new DateTime(2011, 1, 1), service.ResponseTime);
            Assert.Equal("200 : OK", service.StatusCode);
            Assert.Equal("127.0.0.2", service.Ip);
            Assert.Equal("ex123", service.Exception);
        }
    }
}
