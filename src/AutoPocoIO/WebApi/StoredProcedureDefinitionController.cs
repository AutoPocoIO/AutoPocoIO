using AutoPocoIO.CustomAttributes;
using AutoPocoIO.Models;
using AutoPocoIO.Services;
using AutoPocoIO.Api;
#if NETFULL
using System.Web.Http;
using Swashbuckle.Swagger.Annotations;
#else
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authorization;
#endif

namespace AutoPocoIO.WebApi
{
    /// <summary>
    /// Stored Procedure Definition End Points
    /// </summary>
    [DynamicRoutePrefix("api/{connectorName}/_definition/_sproc/{sprocName}")]
    [UseJson]
    public class StoredProcedureDefinitionController : ApiController
    {
        private readonly ILoggingService _loggingService;
        private readonly IStoredProcedureOperations _storedProcedureOperations;
        /// <summary>
        /// Default constructor with logging injected
        /// </summary>
        /// <param name="loggingService">Dependency injected logging for all end points</param>
        public StoredProcedureDefinitionController(IStoredProcedureOperations storedProcedureOperations, ILoggingService loggingService)
        {
            _loggingService = loggingService;
            _storedProcedureOperations = storedProcedureOperations;
        }

        /// <summary>
        /// List all parameters
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="sprocName">Stored Procedure name in the database.</param>
        [Route("")]
        [HttpGet]
        [SwaggerOperation("getSprocDefinition")]
        [SwaggerResponse(200, "List of all parameters used in the Stored Procedure", typeof(StoredProcedureDefinition))]
        public StoredProcedureDefinition Get(string connectorName, string sprocName) =>
            _storedProcedureOperations.Definition(connectorName, sprocName, _loggingService);

        /// <summary>
        /// Get a single parameter
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="sprocName">Stored Procedure name in the database.</param>
        /// <param name="paramName">Parameters name in the database</param>
        [Route("{paramName}")]
        [SwaggerOperation("getSprocParameterDefinition")]
        [HttpGet]
        [SwaggerResponse(200, "Definition of a single parameter", typeof(StoredProcedureParameterDefinition))]
        public StoredProcedureParameterDefinition Get(string connectorName, string sprocName, string paramName) =>
            _storedProcedureOperations.Definition(connectorName, sprocName, paramName, _loggingService);
    }
}