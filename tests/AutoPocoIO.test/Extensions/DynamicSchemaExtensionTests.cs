using AutoPocoIO.Context;
using AutoPocoIO.DynamicSchema.Db;
using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.Factories;
using AutoPocoIO.Models;
using AutoPocoIO.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Concurrent;
using Xunit;

namespace AutoPocoIO.test.Extensions
{
    [Trait("Category", TestCategories.Unit)]
    public class DynamicSchemaExtensionTests
    {
        private readonly Config config;
        private readonly Mock<OperationResource> resource;
        private readonly Mock<IDbSchemaBuilder> schemaBuilder;
        private readonly Mock<ISchemaInitializer> schemaInitializer;
        private readonly DbContextOptions<AppDbContext> appDbOptions;
        
        public DynamicSchemaExtensionTests()
        {
            config = new Config();
            schemaBuilder = new Mock<IDbSchemaBuilder>();
            schemaInitializer = new Mock<ISchemaInitializer>();

            appDbOptions = new DbContextOptionsBuilder<AppDbContext>()
              .UseInMemoryDatabase(databaseName: "appDb" + Guid.NewGuid().ToString())
              .Options;

            var connStringFactory = new Mock<IConnectionStringFactory>();
            connStringFactory.Setup(c => c.GetConnectionInformation(1, "connStr"))
                .Returns(new ConnectionInformation());

            var service = new ServiceCollection()
               .AddSingleton(config)
               .AddSingleton(schemaBuilder.Object)
               .AddSingleton(schemaInitializer.Object)
               .AddSingleton(new AppDbContext(appDbOptions))
               .AddTransient(c => connStringFactory.Object)
               .BuildServiceProvider();

            PrivateObject authProvider = new PrivateObject(ServiceProviderCache.Instance);
            var dictionary = (ConcurrentDictionary<ResourceType, IServiceProvider>)authProvider.GetField("_configurations");
            dictionary.Clear();
            dictionary.GetOrAdd(ResourceType.None, service);

            resource = new Mock<OperationResource>(service)
            {
                CallBase = true
            };


        }

        [FactWithName]
        public void LoadAdapter()
        {
            var connector = new Models.Connector
            {
                Name = "aa",
                Schema = "sch1",
                ResourceType = 1,
                ConnectionStringDecrypted = "connStr"
            };

            resource.Object.ConfigureAction(connector,OperationType.read, "obj");
            resource.Object.LoadDbAdapter();

            Assert.Equal("sch1", config.FilterSchema);
            Assert.Equal("aa", config.DatabaseConnectorName);
            Assert.Equal("obj", config.IncludedTable);

            schemaInitializer.Verify(c => c.Initilize(), Times.Once);
        }

        [FactWithName]
        public void LoadSchema()
        {
            var connector = new Models.Connector
            {
                Name = "aa",
                Schema = "sch1",
                ResourceType = 1,
                ConnectionStringDecrypted = "connStr"
            };

            resource.Object.ConfigureAction(connector,OperationType.read, "obj");
            resource.Object.LoadSchema();

            Assert.Equal("sch1", config.FilterSchema);
            Assert.Equal("aa", config.DatabaseConnectorName);
             Assert.Null(config.IncludedTable);

            schemaInitializer.Verify(c => c.Initilize(), Times.Once);
        }

        [FactWithName]
        public void LoadProc()
        {
            var connector = new Models.Connector
            {
                Name = "aa",
                Schema = "sch1",
                ResourceType = 1,
                ConnectionStringDecrypted = "connStr"
            };

            resource.Object.ConfigureAction(connector,OperationType.read, "obj");
            resource.Object.LoadProc("schema1", "proc2");

            Assert.Equal("schema1", config.FilterSchema);
             Assert.Null(config.IncludedTable);
            Assert.Equal("proc2", config.IncludedStoredProcedure);

            schemaInitializer.Verify(c => c.Initilize(), Times.Once);
        }
    }
}
