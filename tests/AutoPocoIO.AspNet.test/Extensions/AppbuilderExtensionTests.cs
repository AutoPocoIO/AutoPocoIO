using AutoPocoIO.Extensions;
using AutoPocoIO.Models;
using AutoPocoIO.Services;
using Microsoft.AspNet.OData.Query;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin.Builder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Dependencies;

namespace AutoPocoIO.AspNet.test.Extensions
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class AppbuilderExtensionTests
    {
        private readonly AppBuilder builder = new AppBuilder();
        private readonly HttpConfiguration config = new HttpConfiguration();

        [TestInitialize]
        public void Init()
        {
            builder.Properties.Add("host.AppName", "app1");

            var resolver = new Mock<IDependencyResolver>();
            resolver.Setup(c => c.GetService(typeof(IAppDatabaseSetupService))).Returns(Mock.Of<IAppDatabaseSetupService>());
            config.DependencyResolver = resolver.Object;
        }

        [TestMethod]
        public void UseDashboardSetsPathToAutoPoco()
        {
            builder.UseAutoPoco(config);
            Assert.AreEqual("autopoco", AutoPocoConfiguration.DashboardPathPrefix);
        }

        [TestMethod]

        public void UseDashboardSetsPathToDashPath()
        {
            var options = new AutoPocoOptions
            {
                DashboardPath = "/dashPath123"
            };

            builder.UseAutoPoco(config, options);
            Assert.AreEqual("dashPath123", AutoPocoConfiguration.DashboardPathPrefix);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UseDashboardChecksForConfig()
        {
            var options = new AutoPocoOptions();
            builder.UseAutoPoco(null, options);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DashPathMustBeMore1Char()
        {
            var options = new AutoPocoOptions
            {
                DashboardPath = "a"
            };

            builder.UseAutoPoco(config, options);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DashPathMustStartWithSlash()
        {
            var options = new AutoPocoOptions
            {
                DashboardPath = "dash"
            };

            builder.UseAutoPoco(config, options);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UseDashboardChecksForPath()
        {
            builder.UseAutoPoco(config, null);
        }

        [TestMethod]
        public void UseAutoPocoSetsUpOdataOnHttpConfig()
        {
            builder.UseAutoPoco(config);

            Assert.IsInstanceOfType(config.Properties.First(c => (string)c.Key == "Microsoft.AspNet.OData.NonODataRootContainerKey").Value, typeof(ServiceProvider));
            Assert.IsInstanceOfType(config.Properties.First(c => (string)c.Key == "Microsoft.AspNet.OData.PerRouteContainerKey").Value, typeof(Microsoft.AspNet.OData.PerRouteContainer));

            DefaultQuerySettings odataSettings = (DefaultQuerySettings)config.Properties.First(c => (string)c.Key == "Microsoft.AspNet.OData.DefaultQuerySettings").Value;

            Assert.IsTrue(odataSettings.EnableCount);
            Assert.IsTrue(odataSettings.EnableOrderBy);
            Assert.IsTrue(odataSettings.EnableExpand);
            Assert.IsTrue(odataSettings.EnableSelect);
            Assert.AreEqual(1000, odataSettings.MaxTop);
        }
    }
}