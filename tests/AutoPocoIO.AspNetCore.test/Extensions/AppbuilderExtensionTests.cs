using AutoPocoIO.Context;
using AutoPocoIO.Extensions;
using AutoPocoIO.Models;
using AutoPocoIO.Services;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace AutoPocoIO.AspNetCore.test.Extensions
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class AppbuilderExtensionTests
    {
        private IApplicationBuilder builder;
        private DbContextOptions<AppDbContext> appDbOptions;

        private static IServiceCollection startupServices;
        private class TestStartup
        {
            public void ConfigureServices(IServiceCollection services)
            {
                startupServices = services;
            }
#pragma warning disable IDE0060 // Remove unused parameter
            public void Configure(IApplicationBuilder app, IHostingEnvironment env)
#pragma warning restore IDE0060 // Remove unused parameter
            {
            }
        }

        [TestInitialize]
        public void Init()
        {
            appDbOptions = new DbContextOptionsBuilder<AppDbContext>()
             .UseInMemoryDatabase(databaseName: "appDb" + Guid.NewGuid().ToString())
             .Options;

            var db = new AppDbContext(appDbOptions);

            //Configure with default .net core services
            startupServices = new ServiceCollection();
            WebHost.CreateDefaultBuilder(new string[0]).UseStartup<TestStartup>().Build();

            var serviceCollection = startupServices;
            serviceCollection.AddSingleton(db);
            serviceCollection.AddOData();
            serviceCollection.AddMvcCore();
            serviceCollection.AddSingleton(Mock.Of<IAppDatabaseSetupService>());

            builder = new ApplicationBuilder(serviceCollection.BuildServiceProvider());
        }

        [TestMethod]
        public void UseDashboardSetsPathToAutoPoco()
        {
            builder.UseAutoPoco();
            Assert.AreEqual("autopoco", AutoPocoConfiguration.DashboardPathPrefix);
        }

        [TestMethod]

        public void UseDashboardSetsPathToDashPath()
        {
            var options = new AutoPocoOptions
            {
                DashboardPath = "/dashPath123"
            };
            builder.UseAutoPoco(options);
            Assert.AreEqual("dashPath123", AutoPocoConfiguration.DashboardPathPrefix);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DashPathMustBeMore1Char()
        {
            var options = new AutoPocoOptions
            {
                DashboardPath = "a"
            };
            builder.UseAutoPoco(options);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DashPathMustStartWithSlash()
        {
            var options = new AutoPocoOptions
            {
                DashboardPath = "dash"
            };
            builder.UseAutoPoco(options);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UseDashboardChecksForPath()
        {
            builder.UseAutoPoco(null);
        }

        [TestMethod]
        public void UseAutoPocoSetsUpOdata()
        {
            builder.UseAutoPoco();

            Assert.IsNotNull(builder.ApplicationServices.GetService<ODataOptions>());
            Assert.IsNotNull(builder.ApplicationServices.GetService<IPerRouteContainer>());

            DefaultQuerySettings odataSettings = builder.ApplicationServices.GetService<DefaultQuerySettings>();

            Assert.IsTrue(odataSettings.EnableCount);
            Assert.IsTrue(odataSettings.EnableOrderBy);
            Assert.IsTrue(odataSettings.EnableExpand);
            Assert.IsTrue(odataSettings.EnableSelect);
            Assert.AreEqual(1000, odataSettings.MaxTop);
        }



        //[TestMethod]
        //public void UseSqlServerWithEncryption()
        //{
        //    var dbSetup = new Mock<IAppDatabaseSetupService>();
        //    dbSetup.Setup(c => c.SetupEncryption("slt", "key", 123)).Verifiable();
        //    dbSetup.Setup(c => c.Migrate(ResourceType.Mssql)).Verifiable();


        //    var services = new ServiceCollection();
        //    services.AddSingleton(dbSetup.Object);

        //    IApplicationBuilder builder = new ApplicationBuilder(services.BuildServiceProvider());

        //    builder.UseSqlServer("slt", "key", 123);

        //    dbSetup.Verify();
        //}

        //[TestMethod]
        //public void UseSqlServerWithoutEncryption()
        //{
        //    var dbSetup = new Mock<IAppDatabaseSetupService>();

        //    dbSetup.Setup(c => c.Migrate(ResourceType.Mssql)).Verifiable();

        //    var services = new ServiceCollection();
        //    services.AddSingleton(dbSetup.Object);

        //    IApplicationBuilder builder = new ApplicationBuilder(services.BuildServiceProvider());

        //    builder.UseSqlServer();

        //    dbSetup.Verify();
        //    dbSetup.Verify(c => c.SetupEncryption(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        //}

        //[TestMethod]
        //[ExpectedException(typeof(ArgumentNullException))]
        //public void CheckDepResolverNotNullForUseSqlEncryption()
        //{
        //    IApplicationBuilder dependencyResolver = null;
        //    dependencyResolver.UseSqlServer("slt", "key", 123);
        //}

        //[TestMethod]
        //[ExpectedException(typeof(ArgumentNullException))]
        //public void CheckSaltNotNullForUseSqlEncryption()
        //{
        //    var services = new ServiceCollection();
        //    IApplicationBuilder builder = new ApplicationBuilder(services.BuildServiceProvider());

        //    builder.UseSqlServer(null, "key", 123);
        //}

        //[TestMethod]
        //[ExpectedException(typeof(ArgumentNullException))]
        //public void CheckSecretKeyNotNullForUseSqlEncryption()
        //{
        //    var services = new ServiceCollection();
        //    IApplicationBuilder builder = new ApplicationBuilder(services.BuildServiceProvider());

        //    builder.UseSqlServer("slt", null, 123);
        //}

        //[TestMethod]
        //[ExpectedException(typeof(ArgumentNullException))]
        //public void CheckDepResolverNotNullForUseSql()
        //{
        //    IApplicationBuilder dependencyResolver = null;
        //    dependencyResolver.UseSqlServer();
        //}
    }
}
