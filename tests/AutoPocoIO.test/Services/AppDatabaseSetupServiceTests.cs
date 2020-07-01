using AutoPocoIO.Context;
using AutoPocoIO.Extensions;
using AutoPocoIO.Factories;
using AutoPocoIO.Migrations;
using AutoPocoIO.Models;
using AutoPocoIO.Resources;
using AutoPocoIO.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace AutoPocoIO.test.Services
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class AppDatabaseSetupServiceTests
    {
        readonly string connString = $"appDbPro{Guid.NewGuid()}";
        AppDatabaseSetupService setupService;
        Mock<IMigrationsAssembly> mig;
        Mock<IHistoryRepository> applied;
        Mock<AppDbContext> appDb;
        List<string> migrationCalled;

        [TestInitialize]
        public void Initialize()
        {
            migrationCalled = new List<string>();

            mig = new Mock<IMigrationsAssembly>();


            applied = new Mock<IHistoryRepository>();
            var migrator = new Mock<IMigrator>();
            migrator.Setup(c => c.Migrate(It.IsAny<string>()))
                .Callback<string>(c => migrationCalled.Add(c));

            var connection = new Mock<DbConnection>();
            connection.Setup(c => c.ConnectionString).Returns("conn1");
            var connService = new Mock<IRelationalConnection>();
            connService.Setup(c => c.DbConnection).Returns(connection.Object);

            var services = new ServiceCollection();
            services.AddEntityFrameworkInMemoryDatabase();
            services.AddSingleton(mig.Object);
            services.AddSingleton(applied.Object);
            services.AddSingleton(migrator.Object);
            services.AddSingleton(connService.Object);

            var appOptionBuilder = new DbContextOptionsBuilder<AppDbContext>();
            appOptionBuilder.UseInMemoryDatabase(databaseName: connString);
            appOptionBuilder.UseInternalServiceProvider(services.BuildServiceProvider());

            appDb = new Mock<AppDbContext>(appOptionBuilder.Options) { CallBase = true };

            appDb.Object.Connector.AddRange(
                new Connector { Id = "1", Name = "appDb" },
                new Connector { Id = "3", Name = "logDb" }
                );
            appDb.Object.SaveChanges();

            var migContextOptions = new DbContextOptionsBuilder<AppMigrationContext>();
            migContextOptions.UseInMemoryDatabase(databaseName: connString);
            migContextOptions.UseInternalServiceProvider(services.BuildServiceProvider());

            var appMigDb = new Mock<AppMigrationContext>(migContextOptions.Options) { CallBase = true };
            var logMigDb = new Mock<LoggingMigrationContext>();

            var appDbfacade = new Mock<DatabaseFacade>(appDb.Object);
            var logDbfacade = new Mock<DatabaseFacade>(appDb.Object);
            appMigDb.Setup(c => c.Database).Returns(appDbfacade.Object);
            logMigDb.Setup(c => c.Database).Returns(logDbfacade.Object);

            var factory = new Mock<IConnectionStringFactory>();
            factory.Setup(c => c.GetConnectionInformation(1, "conn1"))
                .Returns(new ConnectionInformation { InitialCatalog = "cat1" });

            setupService = new AppDatabaseSetupService(logMigDb.Object,
                                                         appMigDb.Object,
                                                         appDb.Object,
                                                         factory.Object);
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

        //[TestMethod]
        //public void SetConnectionStringDuringBasicMigration()
        //{
        //    mig.Setup(c => c.Migrations).Returns(new Dictionary<string, TypeInfo>());
        //    applied.Setup(c => c.GetAppliedMigrations()).Returns(new List<HistoryRow>());

        //    setupService.Migrate(ResourceType.Mssql);

        //    Assert.AreEqual("cat1", appDb.Object.Connector.First(c => c.Name == "appDb").InitialCatalog);
        //    Assert.AreEqual("cat1", appDb.Object.Connector.First(c => c.Name == "logDb").InitialCatalog);
        //}


        //[TestMethod]
        //public void InitalDb()
        //{
        //    mig.Setup(c => c.Migrations).Returns(new Dictionary<string, TypeInfo>
        //        {
        //            {"100000000000000_AppDb", typeof(AppMigrationContext).GetTypeInfo() },
        //            {"200000000000000_LogDb", typeof(AppMigrationContext).GetTypeInfo() },

        //        });
        //    applied.Setup(c => c.GetAppliedMigrations())
        //        .Returns(new List<HistoryRow>());

        //    setupService.Migrate(ResourceType.Mssql);
        //    Assert.AreEqual(2, migrationCalled.Count());
        //}
    }
}