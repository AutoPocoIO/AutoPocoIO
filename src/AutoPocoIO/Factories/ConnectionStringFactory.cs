using AutoPocoIO.Constants;
using AutoPocoIO.Exceptions;
using AutoPocoIO.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AutoPocoIO.Factories
{
    /// <summary>
    /// Parse connection string from registered <see cref="IConnectionStringBuilder"/>
    /// </summary>
    public class ConnectionStringFactory : IConnectionStringFactory
    {
        private readonly IEnumerable<IConnectionStringBuilder> _builders;

        /// <summary>
        /// Initialize factory with registered <see cref="IConnectionStringBuilder"/>
        /// </summary>
        /// <param name="builders"></param>
        public ConnectionStringFactory(IEnumerable<IConnectionStringBuilder> builders)
        {
            _builders = builders;
        }

        ///<inheritdoc/>
        public ConnectionInformation GetConnectionInformation(DatabaseFacade database)
        {
            Check.NotNull(database, nameof(database));

            IConnectionStringBuilder builder = GetBuilder(database.ProviderName);
            string connectionString = database.GetDbConnection().ConnectionString;
            return builder.ParseConnectionString(connectionString);
        }
        ///<inheritdoc/>
        public string CreateConnectionString(DatabaseFacade database, ConnectionInformation connectionInformation)
        {
            Check.NotNull(database, nameof(database));
            Check.NotNull(connectionInformation, nameof(connectionInformation));

            IConnectionStringBuilder builder = GetBuilder(database.ProviderName);
            return builder.CreateConnectionString(connectionInformation);
        }
        ///<inheritdoc/>
        public ConnectionInformation GetConnectionInformation(string resourceType, string connectionString)
        {
            IConnectionStringBuilder builder = GetBuilder(resourceType);
            return builder.ParseConnectionString(connectionString);
        }
        ///<inheritdoc/>
        public string CreateConnectionString(string resourceType, ConnectionInformation connectionInformation)
        {
            IConnectionStringBuilder builder = GetBuilder(resourceType);
            return builder.CreateConnectionString(connectionInformation);
        }

        private IConnectionStringBuilder GetBuilder(string resourceType)
        {
            IConnectionStringBuilder builder;
            try
            {
                builder = _builders.First(c => c.ResourceType == resourceType);
            }
            catch (InvalidOperationException)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, ExceptionMessages.DbTypeNotRegistered, resourceType), nameof(resourceType));
            }

            return builder;
        }
    }
}