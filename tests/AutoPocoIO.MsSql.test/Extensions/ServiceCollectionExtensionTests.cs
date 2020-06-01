using AutoPocoIO.Context;
using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.Extensions;
using AutoPocoIO.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Linq;
using Xunit;

namespace AutoPocoIO.MsSql.test.Extensions
{

    [Trait("Category", TestCategories.Unit)]
    public class ServiceCollectionExtensionTests
    {
        [FactWithName]
        public void AddSqlServerResourceType()
        {
            var resourceServices = new ServiceCollection();

            PrivateObject authProvider = new PrivateObject(ServiceProviderCache.Instance);
            var dictionary = (ConcurrentDictionary<ResourceType, IServiceProvider>)authProvider.GetField("_configurations");
            dictionary.Clear();
            dictionary.GetOrAdd(ResourceType.Mssql, resourceServices.BuildServiceProvider());

            var services = new ServiceCollection();
            services.WithSqlServerResources();

            Assert.Equal(2, services.Count());

            var provider = services.BuildServiceProvider();
            Assert.NotNull(provider.GetService<IOperationResource>());
            Assert.NotNull(provider.GetService<IConnectionStringBuilder>());
        }

        [FactWithName]
        public void AddSqlServerDatabases()
        {
            var services = new ServiceCollection();
            services.AddScoped<LogDbContext>();
            services.AddScoped<AppDbContext>();
            services.ConfigureSqlServerApplicationDatabase("conn1");

            Assert.Equal(4, services.Count());

            var provider = services.BuildServiceProvider();

            //Dbs
            Assert.NotNull(provider.GetService<LogDbContext>());
            Assert.NotNull(provider.GetService<AppDbContext>());

            //ContextOptions
            Assert.NotNull(provider.GetService<DbContextOptions<LogDbContext>>());
            Assert.NotNull(provider.GetService<DbContextOptions<AppDbContext>>());

            DbContextOptions option = provider.GetService<DbContextOptions<LogDbContext>>();
            Assert.IsType<Microsoft.EntityFrameworkCore.Infrastructure.CoreOptionsExtension>(option.Extensions.ElementAt(0));
            Assert.IsType<Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal.SqlServerOptionsExtension>(option.Extensions.ElementAt(1));

            option = provider.GetService<DbContextOptions<AppDbContext>>();
            Assert.IsType<Microsoft.EntityFrameworkCore.Infrastructure.CoreOptionsExtension>(option.Extensions.ElementAt(0));
            Assert.IsType<Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal.SqlServerOptionsExtension>(option.Extensions.ElementAt(1));
        }
    }
}
