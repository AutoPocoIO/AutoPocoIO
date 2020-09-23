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
    /// <summary>
    /// Set up connection string encyption and migrate database
    /// </summary>
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

        ///<inheritdoc/>
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

        ///<inheritdoc/>
        public void Migrate()
        {
            bool hasAppDbMigration = _appMigrationDb.Database.GetAppliedMigrations().Any(c => c.Equals(MigrationNames.AppDb, StringComparison.OrdinalIgnoreCase));
            MigrateDb(_appMigrationDb);

            //Only change set connection string on first migration
            if (!hasAppDbMigration)
            {
                UpdateConnector(DefaultConnectors.AppDB);
                UpdateConnector(DefaultConnectors.Logging);
            }

            MigrateDb(_logMigrationDb);
        }

        private void MigrateDb(DbContext db)
        {
            IMigrator migrator = db.Database.GetService<IMigrator>();
            migrator.Migrate();
        }

        private void UpdateConnector(string connectorName)
        {
            string connectionString = _appMigrationDb.Database.GetDbConnection().ConnectionString;
            ConnectionInformation connectionInformation = _connectionStringFactory.GetConnectionInformation(_appDbContext.Database);

            var connector = _appDbContext.Connector.SingleOrDefault(c => c.Name == connectorName);
            if (connector != null)
            {
                connector.SetConnectionInfo(connectionInformation);
                connector.ConnectionStringDecrypted = connectionString;
            }

            _appDbContext.SaveChanges();
        }
    }
}
