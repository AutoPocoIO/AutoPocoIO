using AutoPocoIO.Exceptions;
using AutoPocoIO.LoggingMiddleware;
using AutoPocoIO.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AutoPocoIO.AspNetCore.test.Middleware
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class LogRequestAndResponseMiddlewareTests
    {
        public class AddLoggingDataTestMiddleware
        {
            private readonly RequestDelegate _next;
            public AddLoggingDataTestMiddleware(RequestDelegate next)
            {
                _next = next;
            }

            public Task Invoke(HttpContext context)
            {
                context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.2");
                context.Response.Headers.Add("hdr1", new[] { "val" });

                Mock<ClaimsIdentity> identity = new Mock<ClaimsIdentity>();
                identity.Setup(c => c.Name).Returns("usr");

                context.User = new ClaimsPrincipal(identity.Object);
                context.Request.Headers.Add("User-Agent", new[] { "agent1" });
                return _next.Invoke(context);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public class ThrowExceptionMiddleware
        {
            readonly Exception ex;
            public ThrowExceptionMiddleware(RequestDelegate next, Exception ex)
            {
                this.ex = ex;
            }


            public Task Invoke(HttpContext context)
            {
                throw ex;
            }
        }

        private class TestStartup
        {

            public void ConfigureServices(IServiceCollection services)
            {
                services.AddSingleton(logger.Object);

                Mock<IServiceScopeFactory> scopeProvider = new Mock<IServiceScopeFactory>();
                scopeProvider.Setup(c => c.CreateScope()).Returns(scope.Object);
                services.AddSingleton(scopeProvider.Object);

                Mock<ITimeProvider> timeProvider = new Mock<ITimeProvider>();
                timeProvider.Setup(c => c.UtcNow).Returns(new DateTime(2020, 1, 1));

                services.AddSingleton(timeProvider.Object);
            }
#pragma warning disable IDE0060 // Remove unused parameter
            public void Configure(IApplicationBuilder app, IHostingEnvironment env)
#pragma warning restore IDE0060 // Remove unused parameter
            {
                registerMiddleware(app);
            }
        }

        private static Mock<ILoggingService> logger;
        private static Mock<IServiceScope> scope;
        private static Action<IApplicationBuilder> registerMiddleware;
        IWebHostBuilder builder;

        [TestInitialize]
        public void Init()
        {
            logger = new Mock<ILoggingService>();
            scope = new Mock<IServiceScope>();

            builder = new WebHostBuilder()
                .UseStartup<TestStartup>();

        }

        [TestMethod]
        public void PipelineContinuesAfterNoLogging()
        {
            registerMiddleware = app =>
            {
                app.UseMiddleware<LogRequestAndResponseMiddleware>();
                app.UseMiddleware<EndOfPipeLineTestMiddleware>();
            };

            TestServer testServer = new TestServer(builder);
            HttpClient client = testServer.CreateClient();
            HttpResponseMessage response = client.GetAsync("/").Result;

            Assert.AreEqual(200, (int)response.StatusCode);
            Assert.AreEqual("end of pipeline", response.Content.ReadAsStringAsync().Result);

            logger.Verify(c => c.LogAll(), Times.Never);
        }

        [TestMethod]
        public void PipelineContinuesAfterLogging()
        {
            registerMiddleware = app =>
            {
                app.UseMiddleware<AddLoggingDataTestMiddleware>();
                app.UseMiddleware<LogRequestAndResponseMiddleware>();
                app.UseMiddleware<EndOfPipeLineTestMiddleware>();
            };

            logger.Setup(c => c.LogCount).Returns(1);

            TestServer testServer = new TestServer(builder);
            HttpClient client = testServer.CreateClient();
            HttpResponseMessage response = client.GetAsync("/").Result;

            Assert.AreEqual(200, (int)response.StatusCode);
            Assert.AreEqual("end of pipeline", response.Content.ReadAsStringAsync().Result);
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

            registerMiddleware = app =>
            {
                app.UseMiddleware<AddLoggingDataTestMiddleware>();
                app.UseMiddleware<LogRequestAndResponseMiddleware>();
                app.UseMiddleware<ThrowExceptionMiddleware>(outer.Object);
                app.UseMiddleware<EndOfPipeLineTestMiddleware>();
            };


            logger.SetupAllProperties();
            logger.Setup(c => c.LogCount).Returns(1);

            TestServer testServer = new TestServer(builder);
            HttpClient client = testServer.CreateClient();
            HttpResponseMessage response = client.GetAsync("/").Result;

            Assert.AreEqual("Message: outer\nInner Exception: inner\nStackTrace: stack123", logParameters.Exception);

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

            registerMiddleware = app =>
            {
                app.UseMiddleware<AddLoggingDataTestMiddleware>();
                app.UseMiddleware<LogRequestAndResponseMiddleware>();
                app.UseMiddleware<ThrowExceptionMiddleware>(outer.Object);
                app.UseMiddleware<EndOfPipeLineTestMiddleware>();
            };

            logger.SetupAllProperties();
            logger.Setup(c => c.LogCount).Returns(1);

            TestServer testServer = new TestServer(builder);
            HttpClient client = testServer.CreateClient();
            HttpResponseMessage response = client.GetAsync("/").Result;


            Assert.AreEqual("Message: outer\nInner Exception: \nStackTrace: stack123", logParameters.Exception);

        }

        [TestMethod]
        public void SetBaseCaughtExceptionShowsInResponse()
        {
            ContextLogParameters logParameters = null;
            logger.Setup(c => c.AddContextInfomation(It.IsAny<ContextLogParameters>()))
                    .Callback<ContextLogParameters>(c => logParameters = c);

            var exception = new Mock<BaseCaughtException>();
            exception.Setup(c => c.Message).Returns("exMessage");
            exception.Setup(c => c.StackTrace).Returns("track123");
            exception.Setup(c => c.ResponseCode).Returns(HttpStatusCode.ProxyAuthenticationRequired);
            exception.Setup(c => c.HttpErrorMessage).Returns("test");

            registerMiddleware = app =>
            {
                app.UseMiddleware<AddLoggingDataTestMiddleware>();
                app.UseMiddleware<LogRequestAndResponseMiddleware>();
                app.UseMiddleware<ThrowExceptionMiddleware>(exception.Object);
                app.UseMiddleware<EndOfPipeLineTestMiddleware>();
            };

            Mock<ITimeProvider> timeProvider = new Mock<ITimeProvider>();
            timeProvider.Setup(c => c.UtcNow).Returns(new DateTime(2020, 1, 1));

            logger.SetupAllProperties();
            logger.Setup(c => c.LogCount).Returns(1);

            TestServer testServer = new TestServer(builder);
            HttpClient client = testServer.CreateClient();
            HttpResponseMessage response = client.GetAsync("/").Result;

            Assert.AreEqual("exMessage", logParameters.Exception);
            Assert.AreEqual("test", logParameters.StatusCode);
        }
    }
}
