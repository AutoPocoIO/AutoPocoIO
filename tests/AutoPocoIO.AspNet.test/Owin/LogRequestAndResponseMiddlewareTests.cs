using AutoPocoIO.Extensions;
using AutoPocoIO.LoggingMiddleware;
using AutoPocoIO.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin;
using Microsoft.Owin.Testing;
using Xunit;
using Moq;
using Owin;
using System;
using System.IO;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dependencies;

namespace AutoPocoIO.AspNet.test.Owin
{
    
    [Trait("Category", TestCategories.Unit)]
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

        public LogRequestAndResponseMiddlewareTests()
        {
            Mock<ITimeProvider> timeProvider = new Mock<ITimeProvider>();
            timeProvider.Setup(c => c.UtcNow).Returns(new DateTime(2020, 1, 1));

            var services = new ServiceCollection();
            services.AddSingleton<LogRequestAndResponseMiddleware>();

            Mock<IServiceScopeFactory> scopeProvider = new Mock<IServiceScopeFactory>();
            scopeProvider.Setup(c => c.CreateScope()).Returns(scope.Object);

            var depResolver = new Mock<IDependencyResolver>();
            depResolver.Setup(c => c.GetService(typeof(LogRequestAndResponseMiddleware)))
                .Returns(new LogRequestAndResponseMiddleware(scopeProvider.Object, logger.Object));

            config = new HttpConfiguration
            {
                DependencyResolver = depResolver.Object
            };
        }

        [FactWithName]
        public void PipelineContinuesAfterNoLogging()
        {
            using (var server = TestServer.Create(app =>
            {
                app.UseWithDependencyInjection<LogRequestAndResponseMiddleware>(config);
                app.Use<EndOfPipeLineTestMiddleware>();
            }))
            {
                HttpResponseMessage response = server.HttpClient.GetAsync("/").Result;

                Assert.Equal(200, (int)response.StatusCode);
                Assert.Equal("end of pipeline", response.Content.ReadAsStringAsync().Result);

                logger.Verify(c => c.LogAll(scope.Object), Times.Never);
            }
        }

        [FactWithName]
        public void PipelineContinuesAfterNoLoggingIfLoggerNull()
        {
            var depResolver = new Mock<IDependencyResolver>();
            depResolver.Setup(c => c.GetService(typeof(LogRequestAndResponseMiddleware)))
                .Returns(new LogRequestAndResponseMiddleware(null, null));

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

                Assert.Equal(200, (int)response.StatusCode);
                Assert.Equal("end of pipeline", response.Content.ReadAsStringAsync().Result);

                logger.Verify(c => c.LogAll(scope.Object), Times.Never);
            }
        }

        [FactWithName]
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

                Assert.Equal(200, (int)response.StatusCode);
                Assert.Equal("end of pipeline", response.Content.ReadAsStringAsync().Result);

                logger.Verify(c => c.LogAll(scope.Object), Times.Once);
            }
        }

 
        [FactWithName]
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

                Assert.Equal("Message: outer\nInner Exception: inner\nStackTrace: stack123", logParameters.Exception);
            }
        }

        [FactWithName]
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

                Assert.Equal("Message: outer\nInner Exception: \nStackTrace: stack123", logParameters.Exception);
            }
        }

        [FactWithName]
        public void DisposeOfLogger()
        {
            LogRequestAndResponseMiddleware middleware = new LogRequestAndResponseMiddleware(Mock.Of< IServiceScopeFactory>(), Mock.Of<ILoggingService>());
            PrivateObject obj = new PrivateObject(middleware);

            obj.SetField("RequestBuffer", new MemoryStream());
            obj.SetField("ResponseBuffer", new MemoryStream());

            middleware.Dispose();

            
            Assert.True((bool)obj.GetField("isDisposed"));
        }

        [FactWithName]
        public void DisposeOfLoggerMultiThread()
        {
            LogRequestAndResponseMiddleware middleware = new LogRequestAndResponseMiddleware(Mock.Of<IServiceScopeFactory>(), Mock.Of<ILoggingService>());
            PrivateObject obj = new PrivateObject(middleware);
            obj.SetField("isDisposed", true);

            middleware.Dispose();
        }
    }
}
