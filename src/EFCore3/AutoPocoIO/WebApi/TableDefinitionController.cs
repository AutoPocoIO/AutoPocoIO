using AutoPocoIO.CustomAttributes;
using AutoPocoIO.Services;
using AutoPocoIO.SwaggerAddons;
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
    /// Table Definition End Points
    /// </summary>
    [DynamicRoutePrefix("api/{connectorName}/_definition/_table/{tableName}")]
    [UseJson]
    public class TableDefinitionController : ApiController
    {
        private readonly ILoggingService _loggingService;
        private readonly ITableOperations _tableOperations;
        /// <summary>
        /// Default constructor with logging injected
        /// </summary>
        /// <param name="tableOperations">Access tables</param>
        /// <param name="loggingService">Dependency injected logging for all end points</param>
        public TableDefinitionController(ITableOperations tableOperations, ILoggingService loggingService)
        {
            _loggingService = loggingService;
            _tableOperations = tableOperations;
        }

        /// <summary>
        /// List of columns that exists in a given table
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="tableName">Name of the table in the database.</param>
        [Route("")]
        [SwaggerResponse(200, "List of columns from the table", typeof(SwaggerTableDefType[]))]
        [HttpGet]
        [SwaggerOperation("getTableDefinition")]
        public TableDefinition Get(string connectorName, string tableName) =>
             _tableOperations.Definition(connectorName, tableName, _loggingService);

        /// <summary>
        /// View column attirbutes
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="tableName">Name of the table in the database.</param>
        /// <param name="columnName">Name of the column in the database.</param>
        [Route("{columnName}")]
        [HttpGet]
        [SwaggerOperation("getColumnDefinition")]
        [SwaggerResponse(200, "Retrieve a detailed object with attributes about a specifc column", typeof(SwaggerColumnDefType))]
        public ColumnDefinition Get(string connectorName, string tableName, string columnName) =>
            _tableOperations.Definition(connectorName, tableName, columnName, _loggingService);

    }
}
