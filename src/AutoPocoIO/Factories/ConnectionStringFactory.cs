using AutoPocoIO.Constants;
using AutoPocoIO.DynamicSchema.Enums;
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

            int resourceType = ResourceTypeFromProviderName(database);
            IConnectionStringBuilder builder = GetBuilder(resourceType);
            string connectionString = database.GetDbConnection().ConnectionString;
            return builder.ParseConnectionString(connectionString);
        }
        ///<inheritdoc/>
        public string CreateConnectionString(DatabaseFacade database, ConnectionInformation connectionInformation)
        {
            Check.NotNull(database, nameof(database));
            Check.NotNull(connectionInformation, nameof(connectionInformation));

            int resourceType = ResourceTypeFromProviderName(database);
            IConnectionStringBuilder builder = GetBuilder(resourceType);
            return builder.CreateConnectionString(connectionInformation);
        }
        ///<inheritdoc/>
        public ConnectionInformation GetConnectionInformation(int resourceType, string connectionString)
        {
            IConnectionStringBuilder builder = GetBuilder(resourceType);
            return builder.ParseConnectionString(connectionString);
        }
        ///<inheritdoc/>
        public string CreateConnectionString(int resourceType, ConnectionInformation connectionInformation)
        {
            IConnectionStringBuilder builder = GetBuilder(resourceType);
            return builder.CreateConnectionString(connectionInformation);
        }

        private IConnectionStringBuilder GetBuilder(int resourceType)
        {
            IConnectionStringBuilder builder;
            if (Enum.IsDefined(typeof(ResourceType), resourceType))
            {
                ResourceType type = (ResourceType)resourceType;

                try
                {
                    builder = _builders.First(c => c.ResourceType == type);
                }
                catch (InvalidOperationException)
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, ExceptionMessages.DbTypeNotRegistered, (ResourceType)resourceType), nameof(resourceType));
                }
            }
            else
                throw new ArgumentOutOfRangeException(ExceptionMessages.DbAdapterNotFound);

            return builder;
        }

        private int ResourceTypeFromProviderName(DatabaseFacade database)
        {
            string providerName = database.ProviderName;
            switch (providerName)
            {
                case "Microsoft.EntityFrameworkCore.SqlServer":
                    return (int)ResourceType.Mssql;
                case "Microsoft.EntityFrameworkCore.MySql":
                    return (int)ResourceType.Mysql;
                case "Microsoft.EntityFrameworkCore.Oracle":
                    return (int)ResourceType.Oracle;
            }

            return 0;
        }


    }
}