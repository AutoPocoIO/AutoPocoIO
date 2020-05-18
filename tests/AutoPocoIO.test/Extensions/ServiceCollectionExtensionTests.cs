using AutoPocoIO.Api;
using AutoPocoIO.Context;
using AutoPocoIO.Extensions;
using AutoPocoIO.Factories;
using AutoPocoIO.LoggingMiddleware;
using AutoPocoIO.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace AutoPocoIO.test.Extensions
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class ServiceCollectionExtensionTests
    {
        private void AddDatabaseDependency(ServiceCollection services)
        {
            services.AddSingleton<AppDbContext>();
            services.AddSingleton<LogDbContext>();
            services.AddSingleton<DbContextOptions<AppDbContext>>();
            services.AddSingleton<DbContextOptions<LogDbContext>>();
        }


        [TestMethod]
        public void AddAutoPocoServices()
        {
            var services = new ServiceCollection();
            services.AddAutoPoco();

            var provider = services.BuildServiceProvider();

#if NETFULL
            Assert.AreEqual(23, services.Count());
#else
            Assert.AreEqual(121, services.Count());
#endif
        }


        [TestMethod]
        public void AddAutoPocoServicesDatabase()
        {
            var services = new ServiceCollection();
            services.AddAutoPoco();
            AddDatabaseDependency(services);
            var provider = services.BuildServiceProvider();

            //Get AutoPoco services
            //Ops
            Assert.IsNotNull(provider.GetService<ITableOperations>());
            Assert.IsNotNull(provider.GetService<IViewOperations>());
            Assert.IsNotNull(provider.GetService<IStoredProcedureOperations>());
            Assert.IsNotNull(provider.GetService<ISchemaOperations>());

            //Resources
            Assert.IsNotNull(provider.GetService<IResourceFactory>());
            Assert.IsNotNull(provider.GetService<IAppAdminService>());
            Assert.IsNotNull(provider.GetService<IRequestQueryStringService>());

            //Logging
            Assert.IsNotNull(provider.GetService<ITimeProvider>());
            Assert.IsNotNull(provider.GetService<ILoggingService>());


            //Db Access
            Assert.IsNotNull(provider.GetService<IConnectionStringFactory>());
            Assert.IsNotNull(provider.GetService<IAppDatabaseSetupService>());

        }

        [TestMethod]
        public void RegisterLoggingMiddleWareServices()
        {
            var services = new ServiceCollection();
            services.AddAutoPoco();

#if NETCORE
            //Not required in core
            var controllers = services.Where(c => c.ImplementationType == typeof(LogRequestAndResponseMiddleware));
            Assert.AreEqual(0, controllers.Count());
#else
            //Must register controllers in framework (cannot get service becuase of owinmiddleware dep)
            var controllers = services.Where(c => c.ImplementationType == typeof(LogRequestAndResponseMiddleware));
            Assert.AreEqual(1, controllers.Count());
#endif
        }

#if NETFULL
        [TestMethod]
        public void RegisterDashbaordServices()
        {
            var services = new ServiceCollection();
          //  services.AddAutDashboard();

            var provider = services.BuildServiceProvider();
            //Assert.IsNotNull(provider.GetService<IConnectionStringFactory>());
        }
#endif


        [TestMethod]
        public void AddDatabaseConfigurationToCollection()
        {

            var services = new ServiceCollection();
            services.AddScoped<LogDbContext>();
            services.AddScoped<AppDbContext>();
            services.ConfigureApplicationDatabase(c => c.UseInMemoryDatabase(databaseName: "db123"));

            Assert.AreEqual(4, services.Count());

            //Assert singleton lifetime
            Assert.AreEqual(ServiceLifetime.Singleton, services.First(c => c.ServiceType == typeof(DbContextOptions<LogDbContext>)).Lifetime);
            Assert.AreEqual(ServiceLifetime.Singleton, services.First(c => c.ServiceType == typeof(DbContextOptions<AppDbContext>)).Lifetime);

            var provider = services.BuildServiceProvider();

            //Dbs
            Assert.IsNotNull(provider.GetService<LogDbContext>());
            Assert.IsNotNull(provider.GetService<AppDbContext>());

            //ContextOptions
            Assert.IsNotNull(provider.GetService<DbContextOptions<LogDbContext>>());
            Assert.IsNotNull(provider.GetService<DbContextOptions<AppDbContext>>());

            DbContextOptions option = provider.GetService<DbContextOptions<LogDbContext>>();
            Assert.IsInstanceOfType(option.Extensions.ElementAt(0), typeof(Microsoft.EntityFrameworkCore.Infrastructure.CoreOptionsExtension));
            Assert.IsInstanceOfType(option.Extensions.ElementAt(1), typeof(Microsoft.EntityFrameworkCore.InMemory.Infrastructure.Internal.InMemoryOptionsExtension));

            option = provider.GetService<DbContextOptions<AppDbContext>>();
            Assert.IsInstanceOfType(option.Extensions.ElementAt(0), typeof(Microsoft.EntityFrameworkCore.Infrastructure.CoreOptionsExtension));
            Assert.IsInstanceOfType(option.Extensions.ElementAt(1), typeof(Microsoft.EntityFrameworkCore.InMemory.Infrastructure.Internal.InMemoryOptionsExtension));
        }
    }
}