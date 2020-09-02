using AutoPocoIO.Context;
using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.Extensions;
using AutoPocoIO.Resources;
using AutoPocoIO.test;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace AutoPocoIO.MsSql.test.Extensions
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class ServiceCollectionExtensionTests
    {
        [TestMethod]
        public void AddSqlServerResourceType()
        {
            var resourceServices = new ServiceCollection();

            PrivateObject authProvider = new PrivateObject(ServiceProviderCache.Instance);
            var dictionary = (ConcurrentDictionary<string, IServiceProvider>)authProvider.GetField("_configurations");
            dictionary.Clear();
            dictionary.GetOrAdd("Microsoft.EntityFrameworkCore.SqlServer", resourceServices.BuildServiceProvider());

            var services = new ServiceCollection();
            services.WithSqlServerResources();

            Assert.AreEqual(2, services.Count());

            var provider = services.BuildServiceProvider();
            Assert.IsNotNull(provider.GetService<IOperationResource>());
            Assert.IsNotNull(provider.GetService<IConnectionStringBuilder>());
        }

        [TestMethod]
        public void AddSqlServerDatabases()
        {
            var services = new ServiceCollection();
            services.AddScoped<LogDbContext>();
            services.AddScoped<AppDbContext>();
            services.ConfigureSqlServerApplicationDatabase("conn1");

            Assert.AreEqual(4, services.Count());

            var provider = services.BuildServiceProvider();

            //Dbs
            Assert.IsNotNull(provider.GetService<LogDbContext>());
            Assert.IsNotNull(provider.GetService<AppDbContext>());

            //ContextOptions
            Assert.IsNotNull(provider.GetService<DbContextOptions<LogDbContext>>());
            Assert.IsNotNull(provider.GetService<DbContextOptions<AppDbContext>>());

            DbContextOptions option = provider.GetService<DbContextOptions<LogDbContext>>();
            Assert.IsInstanceOfType(option.Extensions.ElementAt(0), typeof(Microsoft.EntityFrameworkCore.Infrastructure.CoreOptionsExtension));
            Assert.IsInstanceOfType(option.Extensions.ElementAt(1), typeof(Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal.SqlServerOptionsExtension));

            option = provider.GetService<DbContextOptions<AppDbContext>>();
            Assert.IsInstanceOfType(option.Extensions.ElementAt(0), typeof(Microsoft.EntityFrameworkCore.Infrastructure.CoreOptionsExtension));
            Assert.IsInstanceOfType(option.Extensions.ElementAt(1), typeof(Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal.SqlServerOptionsExtension));
        }
    }
}
