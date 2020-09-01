using AutoPocoIO.Context;
using AutoPocoIO.Dashboard;
using AutoPocoIO.Extensions;
using AutoPocoIO.Owin;
using AutoPocoIO.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Owin;
using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dependencies;


namespace AutoPocoIO.AspNet.test.Owin
{
    [TestClass]
    [TestCategory(TestCategories.Integration)]
    public partial class DashboardMiddlewareTests
    {
        HttpConfiguration config;
        readonly Mock<ILoggingService> logger = new Mock<ILoggingService>();
        readonly Mock<IServiceScope> scope = new Mock<IServiceScope>();

        [TestInitialize]
        public void Init()
        {
            AutoPocoConfiguration.DashboardPathPrefix = "dash123";

            Mock<ITimeProvider> timeProvider = new Mock<ITimeProvider>();
            timeProvider.Setup(c => c.UtcNow).Returns(new DateTime(2020, 1, 1));

            var services = new ServiceCollection();
            services.AddSingleton(Mock.Of<DbContextOptions<AppDbContext>>());
            services.AddSingleton(Mock.Of<DbContextOptions<LogDbContext>>());
            services.AddSingleton<IReplaceServices<DashboardServiceProvider>>(new ReplaceRoutes());
            
            Mock<IServiceScopeFactory> scopeProvider = new Mock<IServiceScopeFactory>();
            scopeProvider.Setup(c => c.CreateScope()).Returns(scope.Object);

            var depResolver = new Mock<IDependencyResolver>();
            depResolver.Setup(c => c.GetService(typeof(DashboardMiddleware)))
                .Returns(new DashboardMiddleware(services.BuildServiceProvider(), Mock.Of<ILoggingService>()));

            config = new HttpConfiguration
            {
                DependencyResolver = depResolver.Object
            };


        }

        [TestMethod]
        public void ReturnFoundPage()
        {
            using (var server = TestServer.Create(app =>
            {
                app.UseWithDependencyInjection<DashboardMiddleware>(config);
                app.Use<EndOfPipeLineTestMiddleware>();
            }))
            {
                HttpResponseMessage response = server.HttpClient.GetAsync("/dash123/forGet").Result;
                Assert.AreEqual(200, (int)response.StatusCode);
                Assert.AreEqual("testPageInfo", response.Content.ReadAsStringAsync().Result);
            }
        }

        [TestMethod]
        public void ReturnFoundPageParseQueryString()
        {
            using (var server = TestServer.Create(app =>
            {
                app.UseWithDependencyInjection<DashboardMiddleware>(config);
                app.Use<EndOfPipeLineTestMiddleware>();
            }))
            {
                HttpResponseMessage response = server.HttpClient.GetAsync("/dash123/forGet?test=123").Result;
                Assert.AreEqual(200, (int)response.StatusCode);
                Assert.AreEqual("testPageInfotest123", response.Content.ReadAsStringAsync().Result);
            }
        }

        [TestMethod]
        public void SetErrorCodeIfWrongRequestType()
        {
            using (var server = TestServer.Create(app =>
            {
                app.UseWithDependencyInjection<DashboardMiddleware>(config);
                app.Use<EndOfPipeLineTestMiddleware>();
            }))
            {
                HttpResponseMessage response = server.HttpClient.PostAsync("/dash123/forGet", new StringContent("")).Result;
                Assert.AreEqual(405, (int)response.StatusCode);
            }
        }

        [TestMethod]
        public void SkipDashboardIfRouteNotFound()
        {
            using (var server = TestServer.Create(app =>
            {
                app.UseWithDependencyInjection<DashboardMiddleware>(config);
                app.Use<EndOfPipeLineTestMiddleware>();
            }))
            {
                HttpResponseMessage response = server.HttpClient.GetAsync("/dash123/missingRoute").Result;
                Assert.AreEqual("end of pipeline", response.Content.ReadAsStringAsync().Result);
            }
        }

        [TestMethod]
        public void SkipDashboardIfPrefixPathNotFound()
        {
            using (var server = TestServer.Create(app =>
            {
                app.UseWithDependencyInjection<DashboardMiddleware>(config);
                app.Use<EndOfPipeLineTestMiddleware>();
            }))
            {
                HttpResponseMessage response = server.HttpClient.GetAsync("/otherPrefix/forGet").Result;
                Assert.AreEqual("end of pipeline", response.Content.ReadAsStringAsync().Result);
            }
        }
    }
}
