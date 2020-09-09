using AutoPocoIO.Factories;
using AutoPocoIO.Models;
using AutoPocoIO.Services;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace AutoPocoIO.Api
{
    /// <summary>
    ///  API for accessings stored procedures.
    /// </summary>
    public interface IStoredProcedureOperations
    {
        /// <summary>
        /// Factory to find the correct database resource
        /// </summary>
        IResourceFactory ResourceFactory { get; }

        /// <summary>
        /// Get procedure parameters.
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="procedureName">Name of procedure.</param>
        /// <param name="loggingService">Optional. Log operation if not null.</param>
        /// <returns></returns>
        StoredProcedureDefinition Definition(string connectorName, string procedureName, ILoggingService loggingService = null);
        /// <summary>
        /// Get detailed information about a specific parameter
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="procedureName">Name of procedure.</param>
        /// <param name="parameterName">Name of parameter.</param>
        /// <param name="loggingService">Optional. Log operation if not null.</param>
        /// <returns></returns>
        StoredProcedureParameterDefinition Definition(string connectorName, string procedureName, string parameterName, ILoggingService loggingService = null);
        /// <summary>
        /// Execute a stored procedure that uses no parameters
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="procedureName">Name of procedure.</param>
        /// <param name="loggingService">Optional. Log operation if not null.</param>
        /// <returns></returns>
        IDictionary<string, object> ExecuteNoParameters(string connectorName, string procedureName, ILoggingService loggingService = null);
        /// <summary>
        /// Execute a stored proecedure from webapi.  
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="procedureName">Name of procedure.</param>
        /// <param name="parameters">Json representation of the parameters.</param>
        /// <param name="loggingService">Optional. Log operation if not null.</param>
        /// <returns></returns>
        IDictionary<string, object> Execute(string connectorName, string procedureName, JToken parameters, ILoggingService loggingService = null);
        /// <summary>
        /// Execute web api from a view model 
        /// </summary>
        /// <typeparam name="TViewModel">Model of stored procedure parameters</typeparam>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="procedureName">Name of procedure.</param>
        /// <param name="parameters">View model mapped parameters</param>
        /// <param name="loggingService">Optional. Log operation if not null.</param>
        /// <returns></returns>
        IDictionary<string, object> Execute<TViewModel>(string connectorName, string procedureName, TViewModel parameters, ILoggingService loggingService = null);
    }
}