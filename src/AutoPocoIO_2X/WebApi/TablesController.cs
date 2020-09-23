using AutoPocoIO.Api;
using AutoPocoIO.CustomAttributes;
using AutoPocoIO.Extensions;
using AutoPocoIO.Services;
using AutoPocoIO.SwaggerAddons;
using System.Linq;
using Newtonsoft.Json.Linq;
using AutoPocoIO.Exceptions;
using System.Runtime.InteropServices;
using System;

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
    /// Table Operation End Points
    /// </summary>
    [DynamicRoutePrefix("api/{connectorName}/_table/{tableName}")]
    [UseJson]
    public class TablesController : ApiController
    {
        private readonly ILoggingService _loggingService;
        private readonly IRequestQueryStringService _queryStringService;
        private readonly ITableOperations _tableOps;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="tableOps">Database table operation</param>
        /// <param name="loggingService">Dependency injected logging for all end points</param>
        /// <param name="queryStringService">Injected service to read http request information</param>
        public TablesController(ITableOperations tableOps, ILoggingService loggingService, IRequestQueryStringService queryStringService)
        {
            _loggingService = loggingService;
            _queryStringService = queryStringService;
            _tableOps = tableOps;
        }
        /// <summary>
        /// Retrieve data from a given table
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="tableName">Name of the table in the database.</param>
        [Route("")]
        [SwaggerResponse(200, "List of values from the table", typeof(IQueryable<SwaggerExampleType>))]
        [HttpGet]
        [SwaggerOperation("getTable")]
        [UseOdataInSwagger]
        public IQueryable<dynamic> Get(string connectorName, string tableName)
        {
            var (list, connectorMax) = _tableOps.GetAll(connectorName, tableName, _loggingService);
            return list.ApplyQuery(connectorMax, _queryStringService.GetQueryStrings());
        }


        /// <summary>
        /// Retrieve single row by primary key
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="tableName">Name of the table in the database.</param>
        /// <param name="id">Primary key.</param>
        /// <returns></returns>
        [Route("{id}")]
        [SwaggerResponse(200, "Single value found by the primary key", typeof(SwaggerExampleType))]
        [SwaggerOperation("getTableById")]
        [HttpGet]
        public object Get(string connectorName, string tableName, string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            return _tableOps.GetById(connectorName, tableName, _loggingService, id.Split(new[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries));
        }


        /// <summary>
        /// Insert a recored into a given table
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="tableName">Name of the table in the database.</param>
        /// <param name="value">JSON object to insert</param>
        /// <returns>Inserted object</returns>
        [Route("")]
        [SwaggerResponse(200, "Inserted object", typeof(SwaggerExampleType))]
        [HttpPost]
        public object Post(string connectorName, string tableName, [FromBody]JToken value) =>
             _tableOps.CreateNewRow(connectorName, tableName, value, _loggingService);


        /// <summary>
        /// Update record in a given table
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="tableName">Name of the table in the database.</param>
        /// <param name="value">JSON object to update.</param>
        /// <returns></returns>
        [Route("{id}")]
        [SwaggerResponse(200, "Updated object", typeof(SwaggerExampleType))]
        [HttpPut]
        public object Put(string connectorName, string tableName, [FromBody]JToken value) =>
             _tableOps.UpdateRow(connectorName, tableName, value, _loggingService);

        /// <summary>
        /// Remove record from a given table
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="tableName">Name of the table in the database.</param>
        /// <param name="id">Primary key.</param>
        /// <returns></returns>
        [Route("{id}")]
        [SwaggerResponse(200, "Deleted object", typeof(SwaggerExampleType))]
        [HttpDelete]
        public object Delete(string connectorName, string tableName, string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            return _tableOps.DeleteRow(connectorName, tableName, _loggingService, id.Split(new[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries));
        }
    }
}
