using AutoPocoIO.Api;
using AutoPocoIO.Context;
using AutoPocoIO.Extensions;
using AutoPocoIO.Factories;
using AutoPocoIO.LoggingMiddleware;
using AutoPocoIO.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Xunit;

namespace AutoPocoIO.test.Extensions
{
    [Trait("Category", TestCategories.Unit)]
    public class ServiceCollectionExtensionTests
    {
        private void AddDatabaseDependency(ServiceCollection services)
        {
            services.AddSingleton<AppDbContext>();
            services.AddSingleton<LogDbContext>();
            services.AddSingleton<DbContextOptions<AppDbContext>>();
            services.AddSingleton<DbContextOptions<LogDbContext>>();
        }


        [FactWithName]
        public void AddAutoPocoServices()
        {
            var services = new ServiceCollection();
            services.AddAutoPoco();

            var provider = services.BuildServiceProvider();

#if NETFULL
            Assert.Equal(23, services.Count());
#else
            Assert.Equal(121, services.Count());
#endif
        }


        [FactWithName]
        public void AddAutoPocoServicesDatabase()
        {
            var services = new ServiceCollection();
            services.AddAutoPoco();
            AddDatabaseDependency(services);
            var provider = services.BuildServiceProvider();

            //Get AutoPoco services
            //Ops
             Assert.NotNull(provider.GetService<ITableOperations>());
             Assert.NotNull(provider.GetService<IViewOperations>());
             Assert.NotNull(provider.GetService<IStoredProcedureOperations>());
             Assert.NotNull(provider.GetService<ISchemaOperations>());

            //Resources
             Assert.NotNull(provider.GetService<IResourceFactory>());
             Assert.NotNull(provider.GetService<IAppAdminService>());
             Assert.NotNull(provider.GetService<IRequestQueryStringService>());

            //Logging
             Assert.NotNull(provider.GetService<ITimeProvider>());
             Assert.NotNull(provider.GetService<ILoggingService>());


            //Db Access
             Assert.NotNull(provider.GetService<IConnectionStringFactory>());
             Assert.NotNull(provider.GetService<IAppDatabaseSetupService>());

        }

        [FactWithName]
        public void RegisterLoggingMiddleWareServices()
        {
            var services = new ServiceCollection();
            services.AddAutoPoco();

#if NETCORE
            //Not required in core
            var controllers = services.Where(c => c.ImplementationType == typeof(LogRequestAndResponseMiddleware));
            Assert.Empty(controllers);
#else
            //Must register controllers in framework (cannot get service becuase of owinmiddleware dep)
            var controllers = services.Where(c => c.ImplementationType == typeof(LogRequestAndResponseMiddleware));
            Assert.Single(controllers);
#endif
        }

#if NETFULL
        [FactWithName]
        public void RegisterDashbaordServices()
        {
            var services = new ServiceCollection();
            services.AddSingleton(new DbContextOptionsBuilder<LogDbContext>()
               .UseInMemoryDatabase(databaseName: "logDb" + Guid.NewGuid().ToString())
               .Options);
            services.AddSingleton(new DbContextOptionsBuilder<AppDbContext>()
              .UseInMemoryDatabase(databaseName: "appDb" + Guid.NewGuid().ToString())
              .Options);

            services.AddAutoPoco();

            var provider = services.BuildServiceProvider();
             Assert.NotNull(provider.GetService<Owin.DashboardMiddleware>());
        }
#endif


        [FactWithName]
        public void AddDatabaseConfigurationToCollection()
        {

            var services = new ServiceCollection();
            services.AddScoped<LogDbContext>();
            services.AddScoped<AppDbContext>();
            services.ConfigureApplicationDatabase(c => c.UseInMemoryDatabase(databaseName: "db123"));

            Assert.Equal(4, services.Count());

            //Assert singleton lifetime
            Assert.Equal(ServiceLifetime.Singleton, services.First(c => c.ServiceType == typeof(DbContextOptions<LogDbContext>)).Lifetime);
            Assert.Equal(ServiceLifetime.Singleton, services.First(c => c.ServiceType == typeof(DbContextOptions<AppDbContext>)).Lifetime);

            var provider = services.BuildServiceProvider();

            //Dbs
             Assert.NotNull(provider.GetService<LogDbContext>());
             Assert.NotNull(provider.GetService<AppDbContext>());

            //ContextOptions
             Assert.NotNull(provider.GetService<DbContextOptions<LogDbContext>>());
             Assert.NotNull(provider.GetService<DbContextOptions<AppDbContext>>());

            DbContextOptions option = provider.GetService<DbContextOptions<LogDbContext>>();
            Assert.IsType<Microsoft.EntityFrameworkCore.Infrastructure.CoreOptionsExtension>(option.Extensions.ElementAt(0));
            Assert.IsType<Microsoft.EntityFrameworkCore.InMemory.Infrastructure.Internal.InMemoryOptionsExtension>(option.Extensions.ElementAt(1));

            option = provider.GetService<DbContextOptions<AppDbContext>>();
            Assert.IsType<Microsoft.EntityFrameworkCore.Infrastructure.CoreOptionsExtension>(option.Extensions.ElementAt(0));
            Assert.IsType<Microsoft.EntityFrameworkCore.InMemory.Infrastructure.Internal.InMemoryOptionsExtension>(option.Extensions.ElementAt(1));
        }
    }
}