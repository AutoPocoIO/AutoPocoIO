using AutoPocoIO.Extensions;
using AutoPocoIO.Models;
using AutoPocoIO.Services;
using Microsoft.AspNet.OData.Query;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin.Builder;
using Xunit;
using Moq;
using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Dependencies;

namespace AutoPocoIO.AspNet.test.Extensions
{
    
    [Trait("Category", TestCategories.Unit)]
    public class AppbuilderExtensionTests
    {
        private readonly AppBuilder builder = new AppBuilder();
        private readonly HttpConfiguration config = new HttpConfiguration();
        public AppbuilderExtensionTests()
        {
            builder.Properties.Add("host.AppName", "app1");

            var resolver = new Mock<IDependencyResolver>();
            resolver.Setup(c => c.GetService(typeof(IAppDatabaseSetupService))).Returns(Mock.Of<IAppDatabaseSetupService>());
            config.DependencyResolver = resolver.Object;
        }

        [FactWithName]
        public void UseDashboardSetsPathToAutoPoco()
        {
            builder.UseAutoPoco(config);
            Assert.Equal("autopoco", AutoPocoConfiguration.DashboardPathPrefix);
        }

        [FactWithName]

        public void UseDashboardSetsPathToDashPath()
        {
            var options = new AutoPocoOptions
            {
                DashboardPath = "/dashPath123"
            };

            builder.UseAutoPoco(config, options);
            Assert.Equal("dashPath123", AutoPocoConfiguration.DashboardPathPrefix);
        }

        [FactWithName]
        public void UseDashboardChecksForConfig()
        {
            var options = new AutoPocoOptions();
            void act() => builder.UseAutoPoco(null, options);
            Assert.Throws<ArgumentNullException>(act);
        }

        [FactWithName]
        public void DashPathMustBeMore1Char()
        {
            var options = new AutoPocoOptions
            {
                DashboardPath = "a"
            };

            void act() => builder.UseAutoPoco(config, options);
            Assert.Throws<ArgumentException>(act);
        }

        [FactWithName]
        public void DashPathMustStartWithSlash()
        {
            var options = new AutoPocoOptions
            {
                DashboardPath = "dash"
            };

            void act() => builder.UseAutoPoco(config, options);
            Assert.Throws<ArgumentException>(act);
        }

        [FactWithName]
        public void UseDashboardChecksForPath()
        {
            void act() => builder.UseAutoPoco(config, null);
            Assert.Throws<ArgumentNullException>(act);
        }

        [FactWithName]
        public void UseAutoPocoSetsUpOdataOnHttpConfig()
        {
            builder.UseAutoPoco(config);

            Assert.IsType<ServiceProvider>(config.Properties.First(c => (string)c.Key == "Microsoft.AspNet.OData.NonODataRootContainerKey").Value);
            Assert.IsType<Microsoft.AspNet.OData.PerRouteContainer>(config.Properties.First(c => (string)c.Key == "Microsoft.AspNet.OData.PerRouteContainerKey").Value);

            DefaultQuerySettings odataSettings = (DefaultQuerySettings)config.Properties.First(c => (string)c.Key == "Microsoft.AspNet.OData.DefaultQuerySettings").Value;

            Assert.True(odataSettings.EnableCount);
            Assert.True(odataSettings.EnableOrderBy);
            Assert.True(odataSettings.EnableExpand);
            Assert.True(odataSettings.EnableSelect);
            Assert.Equal(1000, odataSettings.MaxTop);
        }
    }
}