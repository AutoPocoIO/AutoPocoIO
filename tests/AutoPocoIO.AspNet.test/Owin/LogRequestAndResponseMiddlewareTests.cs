using AutoPocoIO.Extensions;
using AutoPocoIO.LoggingMiddleware;
using AutoPocoIO.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin;
using Microsoft.Owin.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Owin;
using System;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dependencies;

namespace AutoPocoIO.AspNet.test.Owin
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class LogRequestAndResponseMiddlewareTests
    {
        public class AddLoggingDataTestMiddleware : OwinMiddleware
        {
            public AddLoggingDataTestMiddleware(OwinMiddleware next) : base(next)
            {
            }

            public override Task Invoke(IOwinContext context)
            {
                context.Request.RemoteIpAddress = "ip123";
                context.Response.Headers.Add("hdr1", new[] { "val" });

                Mock<ClaimsIdentity> identity = new Mock<ClaimsIdentity>();
                identity.Setup(c => c.Name).Returns("usr");

                context.Authentication.User = new ClaimsPrincipal(identity.Object);
                context.Request.Headers.Add("User-Agent", new[] { "agent1" });
                return Next.Invoke(context);
            }
        }

        public class ThrowExceptionMiddleware : OwinMiddleware
        {
            readonly Exception ex;

            public ThrowExceptionMiddleware(OwinMiddleware next, Exception ex) : base(next)
            {
                this.ex = ex;
            }

            public override Task Invoke(IOwinContext context)
            {
                throw ex;
            }
        }

        readonly Mock<ILoggingService> logger = new Mock<ILoggingService>();
        readonly Mock<IServiceScope> scope = new Mock<IServiceScope>();

        HttpConfiguration config = new HttpConfiguration();

        [TestInitialize]
        public void Init()
        {
            Mock<ITimeProvider> timeProvider = new Mock<ITimeProvider>();
            timeProvider.Setup(c => c.UtcNow).Returns(new DateTime(2020, 1, 1));

            var services = new ServiceCollection();
            services.AddSingleton<LogRequestAndResponseMiddleware>();

            Mock<IServiceScopeFactory> scopeProvider = new Mock<IServiceScopeFactory>();
            scopeProvider.Setup(c => c.CreateScope()).Returns(scope.Object);

            var depResolver = new Mock<IDependencyResolver>();
            depResolver.Setup(c => c.GetService(typeof(LogRequestAndResponseMiddleware)))
                .Returns(new LogRequestAndResponseMiddleware(logger.Object));

            config = new HttpConfiguration
            {
                DependencyResolver = depResolver.Object
            };
        }

        [TestMethod]
        public void PipelineContinuesAfterNoLogging()
        {
            using (var server = TestServer.Create(app =>
            {
                app.UseWithDependencyInjection<LogRequestAndResponseMiddleware>(config);
                app.Use<EndOfPipeLineTestMiddleware>();
            }))
            {
                HttpResponseMessage response = server.HttpClient.GetAsync("/").Result;

                Assert.AreEqual(200, (int)response.StatusCode);
                Assert.AreEqual("end of pipeline", response.Content.ReadAsStringAsync().Result);

                logger.Verify(c => c.LogAll(), Times.Never);
            }
        }

        [TestMethod]
        public void PipelineContinuesAfterNoLoggingIfLoggerNull()
        {
            var depResolver = new Mock<IDependencyResolver>();
            depResolver.Setup(c => c.GetService(typeof(LogRequestAndResponseMiddleware)))
                .Returns(new LogRequestAndResponseMiddleware(null));

            config = new HttpConfiguration
            {
                DependencyResolver = depResolver.Object
            };

            using (var server = TestServer.Create(app =>
            {
                app.UseWithDependencyInjection<LogRequestAndResponseMiddleware>(config);
                app.Use<EndOfPipeLineTestMiddleware>();
            }))
            {
                HttpResponseMessage response = server.HttpClient.GetAsync("/").Result;

                Assert.AreEqual(200, (int)response.StatusCode);
                Assert.AreEqual("end of pipeline", response.Content.ReadAsStringAsync().Result);

                logger.Verify(c => c.LogAll(), Times.Never);
            }
        }

        [TestMethod]
        public void PipelineContinuesAfterLogging()
        {
            using (var server = TestServer.Create(app =>
            {
                app.Use<AddLoggingDataTestMiddleware>();
                app.UseWithDependencyInjection<LogRequestAndResponseMiddleware>(config);
                app.Use<EndOfPipeLineTestMiddleware>();
            }))
            {
                logger.Setup(c => c.LogCount).Returns(1);
                HttpResponseMessage response = server.HttpClient.GetAsync("/").Result;

                Assert.AreEqual(200, (int)response.StatusCode);
                Assert.AreEqual("end of pipeline", response.Content.ReadAsStringAsync().Result);

                logger.Verify(c => c.LogAll(), Times.Once);

            }
        }


        [TestMethod]
        public void SetExceptionInLogger()
        {
            ContextLogParameters logParameters = null;
            logger.Setup(c => c.AddContextInfomation(It.IsAny<ContextLogParameters>()))
                .Callback<ContextLogParameters>(c => logParameters = c);

            var inner = new Exception("inner");
            var outer = new Mock<Exception>("", inner);
            outer.Setup(c => c.Message).Returns("outer");
            outer.Setup(c => c.StackTrace).Returns("stack123");

            using (var server = TestServer.Create(app =>
            {
                app.Use<AddLoggingDataTestMiddleware>();
                app.UseWithDependencyInjection<LogRequestAndResponseMiddleware>(config);
                app.Use<ThrowExceptionMiddleware>(outer.Object);
                app.Use<EndOfPipeLineTestMiddleware>();
            }))
            {
                logger.SetupAllProperties();
                logger.Setup(c => c.LogCount).Returns(1);
                HttpResponseMessage response = server.HttpClient.GetAsync("/").Result;

                Assert.AreEqual("Message: outer\nInner Exception: inner\nStackTrace: stack123", logParameters.Exception);
            }
        }

        [TestMethod]
        public void SetExceptionWithOuterOnlyInLogger()
        {
            ContextLogParameters logParameters = null;
            logger.Setup(c => c.AddContextInfomation(It.IsAny<ContextLogParameters>()))
                .Callback<ContextLogParameters>(c => logParameters = c);

            var outer = new Mock<Exception>("");
            outer.Setup(c => c.Message).Returns("outer");
            outer.Setup(c => c.StackTrace).Returns("stack123");

            using (var server = TestServer.Create(app =>
            {
                app.Use<AddLoggingDataTestMiddleware>();
                app.UseWithDependencyInjection<LogRequestAndResponseMiddleware>(config);
                app.Use<ThrowExceptionMiddleware>(outer.Object);
                app.Use<EndOfPipeLineTestMiddleware>();
            }))
            {
                logger.SetupAllProperties();
                logger.Setup(c => c.LogCount).Returns(1);
                HttpResponseMessage response = server.HttpClient.GetAsync("/").Result;

                Assert.AreEqual("Message: outer\nInner Exception: \nStackTrace: stack123", logParameters.Exception);
            }
        }
    }
}
