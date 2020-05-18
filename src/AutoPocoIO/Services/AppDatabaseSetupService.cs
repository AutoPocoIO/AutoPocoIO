using AutoPocoIO.Context;
using AutoPocoIO.Exceptions;
using AutoPocoIO.Extensions;
using AutoPocoIO.Factories;
using AutoPocoIO.Migrations;
using AutoPocoIO.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Linq;
using static AutoPocoIO.AutoPocoConstants;

namespace AutoPocoIO.Services
{
    internal class AppDatabaseSetupService : IAppDatabaseSetupService
    {
        private readonly LoggingMigrationContext _logMigrationDb;
        private readonly AppMigrationContext _appMigrationDb;
        private readonly AppDbContext _appDbContext;
        private readonly IConnectionStringFactory _connectionStringFactory;

        public AppDatabaseSetupService(LoggingMigrationContext logDb,
                                       AppMigrationContext appDb,
                                       AppDbContext appDbContext,
                                       IConnectionStringFactory connectionStringFactory)
        {
            _logMigrationDb = logDb;
            _appMigrationDb = appDb;
            _appDbContext = appDbContext;
            _connectionStringFactory = connectionStringFactory;
        }

        /// <summary>
        /// Set the encryption settings
        /// </summary>
        /// <param name="encryptionSalt">Must be length 16</param>
        /// <param name="encryptionSecretKey">Must be 128 characters</param>
        /// <param name="cacheTimeoutMinutes">Length in minutes how long database configuration values stay in cache.</param>
        public void SetupEncryption(string encryptionSalt, string encryptionSecretKey, int cacheTimeoutMinutes)
        {
            Check.NotNull(encryptionSalt, nameof(encryptionSalt));
            Check.NotNull(encryptionSecretKey, nameof(encryptionSecretKey));

            if (encryptionSalt.Length != 16)
                throw new ArgumentOutOfRangeException(nameof(encryptionSalt), $"Must be 16 characters. The Salt was {encryptionSalt.Length}.");

            if (encryptionSecretKey.Length != 128)
                throw new ArgumentOutOfRangeException(nameof(encryptionSecretKey), $"Must be 128 characters. The Secret Key was {encryptionSecretKey.Length}.");

            AutoPocoConfiguration.SaltVector = encryptionSalt;
            AutoPocoConfiguration.SecretKey = encryptionSecretKey;
            AutoPocoConfiguration.CacheTimeoutMinutes = cacheTimeoutMinutes;
        }

        public void Migrate()
        {
            MigrateDb(_appMigrationDb);

            string connectionString = _appDbContext.Database.GetDbConnection().ConnectionString;

            ConnectionInformation connectionInformation = _connectionStringFactory.GetConnectionInformation(_appDbContext.Database);

            //Set db connection strings
            var appDbConnector = _appDbContext.Connector.Single(c => c.Name == DefaultConnectors.AppDB);
            appDbConnector.SetConnectionInfo(connectionInformation);
            appDbConnector.ConnectionStringDecrypted = connectionString;

            var logDbConnector = _appDbContext.Connector.Single(c => c.Name == DefaultConnectors.Logging);
            logDbConnector.SetConnectionInfo(connectionInformation);
            logDbConnector.ConnectionStringDecrypted = connectionString;
            _appDbContext.SaveChanges();


            MigrateDb(_logMigrationDb);
        }

        private void MigrateDb(DbContext db)
        {
            IMigrator migrator = db.Database.GetService<IMigrator>();
            migrator.Migrate();
        }
    }
}
