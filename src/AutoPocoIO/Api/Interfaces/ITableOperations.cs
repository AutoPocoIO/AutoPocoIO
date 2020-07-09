using AutoPocoIO.Models;
using AutoPocoIO.Services;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace AutoPocoIO.Api
{
    /// <summary>
    /// Dynamicly access database tables
    /// </summary>
    public interface ITableOperations
    {
        /// <summary>
        /// Insert a recored into a given table
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="tableName">Name of the table in the database.</param>
        /// <param name="value">JSON object to insert</param>
        /// <param name="loggingService">Include logging serivce to log this API call.</param>
        /// <returns>An instance of the object inserted</returns>
        object CreateNewRow(string connectorName, string tableName, JToken value, ILoggingService loggingService = null);
        /// <summary>
        /// Insert a recored into a given table
        /// </summary>
        /// <typeparam name="TViewModel">Type of view model</typeparam>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="tableName">Name of the table in the database.</param>
        /// <param name="value">Object to insert into <paramref name="tableName"/></param>
        /// <param name="loggingService">Include logging serivce to log this API call.</param>
        /// <returns>An instance of the object inserted. </returns>
        TViewModel CreateNewRow<TViewModel>(string connectorName, string tableName, TViewModel value, ILoggingService loggingService = null) where TViewModel : class;
        /// <summary>
        ///  Describes the table and includes list of columns that exists in a given table
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="tableName">Name of the table in the database.</param>
        /// <param name="loggingService">Include logging serivce to log this API call.</param>
        /// <returns>A description of <paramref name="tableName"/></returns>
        TableDefinition Definition(string connectorName, string tableName, ILoggingService loggingService = null);
        /// <summary>
        /// View column attirbutes
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="tableName">Name of the table in the database.</param>
        /// <param name="columnName">Name of the column in the database.</param>
        /// <param name="loggingService">Include logging serivce to log this API call.</param>
        /// <returns>Column attributes</returns>
        ColumnDefinition Definition(string connectorName, string tableName, string columnName, ILoggingService loggingService = null);
        /// <summary>
        /// Remove record from a given table
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="tableName">Name of the table in the database.</param>
        /// <param name="id">Primary Key(s)</param>
        /// <param name="loggingService">Include logging serivce to log this API call.</param>
        /// <returns>The removed object</returns>
        object DeleteRow(string connectorName, string tableName, string id, ILoggingService loggingService = null);
        /// <summary>
        /// Get all records from <paramref name="tableName"/>. Intended for WebAPI controller requests.
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="tableName">Name of the table in the database.</param>
        /// <param name="loggingService">Optional. Include logging serivce to log this API call.</param>
        /// <returns>Dyanamic IQueryable of the results and the connector max.</returns>
        (IQueryable<object> list, int connectorMax) GetAll(string connectorName, string tableName, ILoggingService loggingService = null);
        /// <summary>
        /// Get all records from <paramref name="tableName"/> and projects the to a view model. Intended to be used as the initial part of a linq query.
        /// </summary>
        /// <typeparam name="TViewModel">Type to project the results to.</typeparam>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="tableName">Name of the table in the database.</param>
        /// <param name="loggingService">Include logging serivce to log this API call.</param>
        /// <returns>IQueryable of <typeparamref name="TViewModel"/></returns>
        IQueryable<TViewModel> GetAll<TViewModel>(string connectorName, string tableName, ILoggingService loggingService = null);
        /// <summary>
        /// Retrieves a single record from a table by Primary Key. Note: for composite PKs,
        /// use a semicolon separated string.
        /// </summary>
        /// <param name="connectorName">The name of the connector to the table's schema.</param>
        /// <param name="tableName">The name of the table to retrieve the record from.</param>
        /// <param name="id">The primary key value of the record to be retrieved as a string. 
        /// For composite keys, use semicolon separated string</param>
        /// <param name="loggingService">LoggingService object to log the request. Null by default if no logging is required.</param>
        /// <returns>The record with matching PK. Null if not found.</returns>
        object GetById(string connectorName, string tableName, string id, ILoggingService loggingService = null);
        /// <summary>
        /// Retrieves a single record from a table by Primary Key. Note: for composite PKs,
        /// use a semicolon separated string.
        /// </summary>
        /// <typeparam name="TViewModel">View Model Type</typeparam>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="tableName">Name of the table in the database.</param>
        /// <param name="id">The primary key value of the record to be retrieved as a string. 
        /// For composite keys, use semicolon separated string</param>
        /// <param name="loggingService">Include logging serivce to log this API call.</param>
        /// <returns></returns>
        TViewModel GetById<TViewModel>(string connectorName, string tableName, string id, ILoggingService loggingService = null) where TViewModel : class;
        /// <summary>
        /// Update record in a given table
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="tableName">Name of the table in the database.</param>
        /// <param name="keys">Primary key(s)</param>
        /// <param name="value">JSON object to update</param>
        /// <param name="loggingService">Include logging serivce to log this API call.</param>
        /// <returns>The updated object</returns>
        object UpdateRow(string connectorName, string tableName, string keys, JToken value, ILoggingService loggingService = null);
        /// <summary>
        /// Update record in a given table
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="tableName">Name of the table in the database.</param>
        /// <param name="id">Primary Key(s)</param>
        /// <param name="value">Object to updated in <paramref name="tableName"/></param>
        /// <param name="loggingService">Include logging serivce to log this API call.</param>
        /// <returns>The updated object</returns>
        TViewModel UpdateRow<TViewModel>(string connectorName, string tableName, string id, TViewModel value, ILoggingService loggingService = null) where TViewModel : class;
    }
}