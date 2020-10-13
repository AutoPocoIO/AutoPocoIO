using AutoPocoIO.DynamicSchema.Enums;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace AutoPocoIO.DynamicSchema.Db
{
    /// <summary>
    /// Populate <see cref="IDbSchema"/> with current database configuration
    /// </summary>
    public interface IDbSchemaBuilder
    {
        /// <summary>
        /// Entity framework provider type.
        /// </summary>
        string ResourceType { get; }

        /// <summary>
        /// Create connection from  <see cref="Models.Config.ConnectionString"/>
        /// </summary>
        /// <returns></returns>
        IDbConnection CreateConnection();
        /// <summary>
        /// Create connection from parameter
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <returns></returns>
        IDbConnection CreateConnection(string connectionString);
        /// <summary>
        /// Create options builder with replacement services to remove caching
        /// </summary>
        /// <returns>Configured context options</returns>
        DbContextOptions CreateDbContextOptions();
        /// <summary>
        /// Execute schema command with safe db connection
        /// </summary>
        /// <param name="command">Command to execute</param>
        /// <returns></returns>
        /// <exception cref="Exceptions.OpenConnectorException"></exception>
        DataTable ExecuteSchemaCommand(IDbCommand command);
        /// <summary>
        /// Execute column command
        /// </summary>
        void GetColumns();
        /// <summary>
        /// Execute table and views command
        /// </summary>
        void GetTableViews();
        /// <summary>
        /// Execute stored procedure command
        /// </summary>
        void GetStoredProcedures();
        /// <summary>
        /// Execute list schemas command
        /// </summary>
        void GetSchemas();
    }
}