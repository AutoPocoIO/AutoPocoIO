using AutoPocoIO.Extensions;
using AutoPocoIO.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Net.Http;

namespace AutoPocoIO.AspNetCore.test.Middleware
{
    [TestClass]
    [TestCategory(TestCategories.Integration)]
    public partial class DashboardMiddlewareTests
    {
        private static Mock<ILoggingService> logger;
        private static Mock<IServiceScope> scope;
        private HttpClient client;

        [TestInitialize]
        public void Init()
        {
            logger = new Mock<ILoggingService>();
            scope = new Mock<IServiceScope>();

            IWebHostBuilder builder = new WebHostBuilder()
                .UseStartup<TestStartup>();

            TestServer testServer = new TestServer(builder);
            client = testServer.CreateClient();

            AutoPocoConfiguration.DashboardPathPrefix = "dash123";
        }

        [TestMethod]
        public void ReturnFoundPage()
        {
            HttpResponseMessage response = client.GetAsync("/dash123/forGet").Result;
            Assert.AreEqual(200, (int)response.StatusCode);
            Assert.AreEqual("testPageInfo", response.Content.ReadAsStringAsync().Result);
        }

        [TestMethod]
        public void ReturnFoundPageParseQueryString()
        {
            HttpResponseMessage response = client.GetAsync("/dash123/forGet?test=123").Result;
            Assert.AreEqual(200, (int)response.StatusCode);
            Assert.AreEqual("testPageInfotest123", response.Content.ReadAsStringAsync().Result);
        }

        [TestMethod]
        public void SetErrorCodeIfWrongRequestType()
        {
            HttpResponseMessage response = client.PostAsync("/dash123/forGet", new StringContent("")).Result;
            Assert.AreEqual(405, (int)response.StatusCode);
        }

        [TestMethod]
        public void SkipDashboardIfRouteNotFound()
        {
            HttpResponseMessage response = client.GetAsync("/dash123/missingRoute").Result;
            Assert.AreEqual("end of pipeline", response.Content.ReadAsStringAsync().Result);
        }

        [TestMethod]
        public void SkipDashboardIfPrefixPathNotFound()
        {
            HttpResponseMessage response = client.GetAsync("/otherPrefix/forGet").Result;
            Assert.AreEqual("end of pipeline", response.Content.ReadAsStringAsync().Result);
        }
    }
}
