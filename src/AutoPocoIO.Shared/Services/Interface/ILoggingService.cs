using AutoPocoIO.Constants;
using AutoPocoIO.LoggingMiddleware;
using System;
using System.Threading.Tasks;

namespace AutoPocoIO.Services
{
    /// <summary>
    /// Log resource operation calls
    /// </summary>
    public interface ILoggingService
    {
        /// <summary>
        /// Number of operations ready to log
        /// </summary>
        int LogCount { get; }

        /// <summary>
        /// Request exception
        /// </summary>
        string Exception { get; set; }
        /// <summary>
        /// Request status code
        /// </summary>
        string StatusCode { get; set; }
        /// <summary>
        /// UTC time of response
        /// </summary>
        DateTime ResponseTime { get; set; }
        /// <summary>
        /// Requester's IP address
        /// </summary>
        string Ip { get; set; }

        /// <summary>
        /// Add schema operation to be logged.
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        void AddSchemaToLogger(string connectorName);
        /// <summary>
        /// Add stored procedure operation to be logged.
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="sprocName">Stored procedure name</param>
        /// <param name="httpMethod">Http method type(GET, PUT, POST, DELETE)</param>
        void AddSprocToLogger(string connectorName, string sprocName, HttpMethodType httpMethod);
        /// <summary>
        /// Add table operation to be logged.
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="tableName">Table name</param>
        /// <param name="httpMethod">Http method type(GET, PUT, POST, DELETE)</param>
        void AddTableToLogger(string connectorName, string tableName, HttpMethodType httpMethod);
        /// <summary>
        /// Add table row operation to be logged.
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="tableName"></param>
        /// <param name="httpMethod">Http method type(GET, PUT, POST, DELETE)</param>
        /// <param name="primaryKey"></param>
        void AddTableToLogger(string connectorName, string tableName, HttpMethodType httpMethod, object[] primaryKey);
        /// <summary>
        /// Add view operation to be logged.
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="viewName">View name</param>
        void AddViewToLogger(string connectorName, string viewName);
        /// <summary>
        /// Log all pending operations
        /// </summary>
        /// <returns></returns>
        Task LogAll();
        /// <summary>
        /// Add Http request information to the service
        /// </summary>
        /// <param name="logParameters">Http request parameters to log.</param>
        void AddContextInfomation(ContextLogParameters logParameters);
    }
}