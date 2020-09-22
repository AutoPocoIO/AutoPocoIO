using AutoPocoIO.CustomAttributes;
using Newtonsoft.Json.Linq;
using AutoPocoIO.Services;
using AutoPocoIO.Api;
using System.Collections.Generic;
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
    /// Execute Stored Procedures
    /// </summary>
    [DynamicRoutePrefix("api/{connectorName}/_sproc/{sprocName}")]
    [UseJson]
    public class StoredProcedureController : ApiController
    {
        private readonly ILoggingService _loggingService;
        private readonly IStoredProcedureOperations _storedProcedureOperations;
        /// <summary>
        /// Default constructor with logging injected
        /// </summary>
        /// <param name="storedProcedureOperations">Access stored procedures</param>
        /// <param name="loggingService">Dependency injected logging for all end points</param>
        public StoredProcedureController(IStoredProcedureOperations storedProcedureOperations, ILoggingService loggingService)
        {
            _loggingService = loggingService;
            _storedProcedureOperations = storedProcedureOperations;
        }

        /// <summary>
        /// Execute Stored Procedure (No Params)
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="sprocName">Stored Procedure name in the database.</param>
        [Route("")]
        [HttpGet]
        [SwaggerResponse(200, "Output of the Stored Procedure", typeof(object))]
        public IDictionary<string, object> Get(string connectorName, string sprocName) =>
            _storedProcedureOperations.ExecuteNoParameters(connectorName, sprocName, _loggingService);

        /// <summary>
        /// Execute Stored Procedure (Params)
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="sprocName">Stored Procedure name in the database.</param>
        /// <param name="value">JSON object of parameters.</param>
        [Route("")]
        [HttpPost]
        [SwaggerResponse(200, "Output of the Stored Procedure", typeof(object))]
        public IDictionary<string, object> Post(string connectorName, string sprocName, [FromBody] JToken value) =>
            _storedProcedureOperations.Execute(connectorName, sprocName, value, _loggingService);
    }
}