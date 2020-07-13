using AutoPocoIO.Resources;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace AutoPocoIO.Factories
{
    /// <summary>
    /// Generate connection string from registered <see cref="IConnectionStringBuilder"/>
    /// </summary>
    public interface IConnectionStringFactory
    {
        /// <summary>
        /// Combine connection information to create a connection string base on database type
        /// </summary>
        /// <param name="database">Database used to look up type</param>
        /// <param name="connectionInformation">Values for createing a connection string</param>
        /// <returns></returns>
        string CreateConnectionString(DatabaseFacade database, ConnectionInformation connectionInformation);
        /// <summary>
        /// Combine connection information to create a connection string base on resource type
        /// </summary>
        /// <param name="resourceType">Database type</param>
        /// <param name="connectionInformation">Values for createing a connection string</param>
        /// <returns></returns>
        string CreateConnectionString(int resourceType, ConnectionInformation connectionInformation);
        /// <summary>
        /// Parse connection string based on database type
        /// </summary>
        /// <param name="database">Database used to look up type and connection string</param>
        /// <returns></returns>
        ConnectionInformation GetConnectionInformation(DatabaseFacade database);
        /// <summary>
        /// Parse connection string based on database type
        /// </summary>
        /// <param name="resourceType">Database type</param>
        /// <param name="connectionString">Value to parse</param>
        /// <returns></returns>
        ConnectionInformation GetConnectionInformation(int resourceType, string connectionString);
    }
}