using AutoPocoIO.Context;
using AutoPocoIO.DynamicSchema.Db;
using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.EntityConfiguration;
using AutoPocoIO.Factories;
using AutoPocoIO.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Concurrent;

namespace AutoPocoIO.test.Extensions
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class DynamicSchemaExtensionTests
    {
        private Config config;
        private Mock<OperationResource> resource;
        private Mock<IDbSchemaBuilder> schemaBuilder;
        private Mock<ISchemaInitializer> schemaInitializer;
        private DbContextOptions<AppDbContext> appDbOptions;

        [TestInitialize]
        public void Init()
        {
            config = new Config();
            schemaBuilder = new Mock<IDbSchemaBuilder>();
            schemaInitializer = new Mock<ISchemaInitializer>();

            appDbOptions = new DbContextOptionsBuilder<AppDbContext>()
              .UseInMemoryDatabase(databaseName: "appDb" + Guid.NewGuid().ToString())
              .Options;

            var connStringFactory = new Mock<IConnectionStringFactory>();
            connStringFactory.Setup(c => c.GetConnectionInformation("type1", "connStr"))
                .Returns(new ConnectionInformation());

            var service = new ServiceCollection()
               .AddSingleton(config)
               .AddSingleton(schemaBuilder.Object)
               .AddSingleton(schemaInitializer.Object)
               .AddSingleton(new AppDbContext(appDbOptions))
               .AddTransient(c => connStringFactory.Object)
               .BuildServiceProvider();

            PrivateObject authProvider = new PrivateObject(ServiceProviderCache.Instance);
            var dictionary = (ConcurrentDictionary<string, IServiceProvider>)authProvider.GetField("_configurations");
            dictionary.Clear();
            dictionary.GetOrAdd("type1", service);

            resource = new Mock<OperationResource>(service)
            {
                CallBase = true
            };

            resource.Setup(c => c.ResourceType).Returns("type1");
        }


        [TestMethod]
        public void LoadAdapter()
        {
            var connector = new Models.Connector
            {
                Name = "aa",
                Schema = "sch1",
                ResourceType = "type1",
                ConnectionStringDecrypted = "connStr"
            };

            resource.Object.ConfigureAction(connector, OperationType.read, "obj");
            resource.Object.LoadDbAdapter();

            Assert.AreEqual("sch1", config.FilterSchema);
            Assert.AreEqual("aa", config.DatabaseConnectorName);
            Assert.AreEqual("obj", config.IncludedTable);

            schemaInitializer.Verify(c => c.Initilize(), Times.Once);
        }

        [TestMethod]
        public void LoadSchema()
        {
            var connector = new Models.Connector
            {
                Name = "aa",
                Schema = "sch1",
                ResourceType = "type1",
                ConnectionStringDecrypted = "connStr"
            };

            resource.Object.ConfigureAction(connector, OperationType.read, "obj");
            resource.Object.LoadSchema();

            Assert.AreEqual("sch1", config.FilterSchema);
            Assert.AreEqual("aa", config.DatabaseConnectorName);
            Assert.IsNull(config.IncludedTable);

            schemaInitializer.Verify(c => c.Initilize(), Times.Once);
        }

        [TestMethod]
        public void LoadProc()
        {
            var connector = new Models.Connector
            {
                Name = "aa",
                Schema = "sch1",
                ResourceType = "type1",
                ConnectionStringDecrypted = "connStr"
            };

            resource.Object.ConfigureAction(connector, OperationType.read, "proc2");
            resource.Object.LoadProc();

            Assert.AreEqual("sch1", config.FilterSchema);
            Assert.IsNull(config.IncludedTable);
            Assert.AreEqual("proc2", config.IncludedStoredProcedure);

            schemaInitializer.Verify(c => c.Initilize(), Times.Once);
        }
    }
}
