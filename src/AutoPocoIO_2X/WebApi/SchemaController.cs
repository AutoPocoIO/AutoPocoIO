using AutoPocoIO.CustomAttributes;
using AutoPocoIO.Services;
using AutoPocoIO.Api;
using AutoPocoIO.Models;
#if NETFULL
using System.Web.Http;
using Swashbuckle.Swagger.Annotations;
#else
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
#endif

namespace AutoPocoIO.WebApi
{
    /// <summary>
    /// Schema Definition End Points
    /// </summary>
    [DynamicRoutePrefix("api/{connectorName}/_schema")]
    [UseJson]
    public class SchemaController : ApiController
    {
        private readonly ILoggingService _loggingService;
        private readonly ISchemaOperations _schemaOperations;
        /// <summary>
        /// Default constructor with logging injected
        /// </summary>
        /// <param name="schemaOperations">Access schema definitions operations</param>
        /// <param name="loggingService">Logging for all end points</param>
        public SchemaController(ISchemaOperations schemaOperations, ILoggingService loggingService)
        {
            _loggingService = loggingService;
            _schemaOperations = schemaOperations;
        }

        /// <summary>
        /// List all database objects
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <returns>A list for each object type (Table, View, Stored Procedure)</returns>
        [HttpGet]
        [Route("")]
        [SwaggerResponse(200, "List of objects in the database by type", typeof(SchemaDefinition))]
        public SchemaDefinition Get(string connectorName) =>
            _schemaOperations.Definition(connectorName, _loggingService);
    }
}