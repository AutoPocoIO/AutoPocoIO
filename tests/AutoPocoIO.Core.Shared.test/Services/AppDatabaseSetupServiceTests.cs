using AutoPocoIO.Context;
using AutoPocoIO.EntityConfiguration;
using AutoPocoIO.Extensions;
using AutoPocoIO.Factories;
using AutoPocoIO.Migrations;
using AutoPocoIO.Models;
using AutoPocoIO.Resources;
using AutoPocoIO.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace AutoPocoIO.test.Services
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class AppDatabaseSetupServiceTests
    {
        readonly string connString = "DataSource=appDb" + Guid.NewGuid().ToString();
        AppDatabaseSetupService setupService;
        Mock<IMigrationsAssembly> mig;
        Mock<IHistoryRepository> applied;
        AppDbContext appDb;
        int migrations;

        [TestInitialize]
        public void Initialize()
        {
            AutoPocoConfiguration.SaltVector = "";
            AutoPocoConfiguration.SecretKey = "";


            mig = new Mock<IMigrationsAssembly>();


            applied = new Mock<IHistoryRepository>();
            applied.Setup(c => c.GetAppliedMigrations()).Returns(new List<HistoryRow>());

            var migrator = new Mock<IMigrator>();
            migrator.Setup(c => c.Migrate(It.IsAny<string>()))
                .Callback<string>(c => migrations++);

            var connection = new Mock<DbConnection>();
            connection.Setup(c => c.ConnectionString).Returns("conn1");

            var connService = new Mock<IRelationalConnection>();
            connService.Setup(c => c.DbConnection).Returns(connection.Object);

            var services = new ServiceCollection();
            services.AddEntityFrameworkSqlite();
            services.AddSingleton(mig.Object);
            services.AddSingleton(applied.Object);
            services.AddSingleton(migrator.Object);
            services.AddSingleton(connService.Object);

            var appOptionBuilder = new DbContextOptionsBuilder<AppDbContext>();
            appOptionBuilder.UseInMemoryDatabase(databaseName: connString);

            appDb = new AppDbContext(appOptionBuilder.Options, new ContextEntityConfiguration());

            appDb.Connector.AddRange(
                new Connector { Id = "1", Name = "appDb" },
                new Connector { Id = "3", Name = "logDb" }
                );
            appDb.SaveChanges();

            var migContextOptions = new DbContextOptionsBuilder<AppMigrationContext>()
                 .UseSqlite(connString)
                .UseInternalServiceProvider(services.BuildServiceProvider());

            var migLogContextOptions = new DbContextOptionsBuilder<LoggingMigrationContext>()
                 .UseSqlite(connString)
                .UseInternalServiceProvider(services.BuildServiceProvider());

            var appMigDb = new AppMigrationContext(migContextOptions.Options);
            var logMigDb = new LoggingMigrationContext(migLogContextOptions.Options);

            var factory = new Mock<IConnectionStringFactory>();
            factory.Setup(c => c.GetConnectionInformation(appDb.Database))
                .Returns(new ConnectionInformation { InitialCatalog = "cat1" });

            setupService = new AppDatabaseSetupService(logMigDb,
                                                         appMigDb,
                                                         appDb,
                                                         factory.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            AutoPocoConfiguration.SaltVector = "";
            AutoPocoConfiguration.SecretKey = "";
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowExceptionIfSaltNull()
        {
            setupService.SetupEncryption(null, "asdf", 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowExceptionIfSecretKeyNull()
        {
            setupService.SetupEncryption("adsf", null, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ThrowExceptionIfSaltLengthNot16()
        {
            setupService.SetupEncryption("not16Chars", "401b09eab3c013d4ca54922bb802bec8fd5318192b0a75f201d8b3727429090fb337591abd3e44453b954555b7a0812e1081c39b740293f765eae731f5a65ed1", 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ThrowExceptionIfSecretKeyLengthNot128()
        {
            setupService.SetupEncryption("49OQNVKPAWTMC747", "not128Chars", 1);
        }

        [TestMethod]
        public void SetupEncryptionSetsConfigKeys()
        {
            setupService.SetupEncryption("49OQNVKPAWTMC747", "401b09eab3c013d4ca54922bb802bec8fd5318192b0a75f201d8b3727429090fb337591abd3e44453b954555b7a0812e1081c39b740293f765eae731f5a65ed1", 1);

            Assert.AreEqual("49OQNVKPAWTMC747", AutoPocoConfiguration.SaltVector);
            Assert.AreEqual("401b09eab3c013d4ca54922bb802bec8fd5318192b0a75f201d8b3727429090fb337591abd3e44453b954555b7a0812e1081c39b740293f765eae731f5a65ed1", AutoPocoConfiguration.SecretKey);
            Assert.AreEqual(1, AutoPocoConfiguration.CacheTimeoutMinutes);
        }

        [TestMethod]
        public void RunMigrationForLogAndAppDbContext()
        {
            setupService.Migrate();
            Assert.AreEqual(2, migrations);
        }

        [TestMethod]
        public void UpdateConnectionStringsOnMigrate()
        {
            
            setupService.Migrate();
            Assert.AreEqual("cat1", appDb.Connector.Single(c => c.Name == "appDb").InitialCatalog);
            Assert.AreEqual("cat1", appDb.Connector.Single(c => c.Name == "logDb").InitialCatalog);
        }

        [TestMethod]
        public void SkipSetConnectionStringAfterInitalMigration()
        {
            applied.Setup(c => c.GetAppliedMigrations()).Returns(new List<HistoryRow>() { new HistoryRow("100000000000000_AppDb", "1") });
            setupService.Migrate();
            Assert.IsNull(appDb.Connector.Single(c => c.Name == "appDb").InitialCatalog);
            Assert.IsNull(appDb.Connector.Single(c => c.Name == "logDb").InitialCatalog);
        }
    }
}