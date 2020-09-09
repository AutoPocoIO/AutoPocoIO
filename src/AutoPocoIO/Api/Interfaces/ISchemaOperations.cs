using AutoPocoIO.Factories;
using AutoPocoIO.Models;
using AutoPocoIO.Services;

namespace AutoPocoIO.Api
{
    /// <summary>
    /// API for access database schema structure.
    /// </summary>
    public interface ISchemaOperations
    {
        /// <summary>
        /// Factory to find the correct database resource
        /// </summary>
        IResourceFactory ResourceFactory { get; }
        /// <summary>
        /// Get schema structure.
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="loggingService">Optional. Log operation if not null.</param>
        /// <returns>Lists of all tables, views, and stored procedures.</returns>
        SchemaDefinition Definition(string connectorName, ILoggingService loggingService = null);
    }
}