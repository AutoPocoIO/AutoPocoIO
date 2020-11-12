using AutoPocoIO.CustomAttributes;
using AutoPocoIO.Extensions;
using AutoPocoIO.Services;
using AutoPocoIO.SwaggerAddons;
using System.Linq;
using AutoPocoIO.Exceptions;
using AutoPocoIO.Api;
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
    /// View Operations End Point
    /// </summary>
    [DynamicRoutePrefix("api/{connectorName}/_view/{viewName}")]
    [UseJson]
    public class ViewsController : ApiController
    {
        private readonly ILoggingService _loggingService;
        private readonly IViewOperations _viewOperations;
        private readonly IRequestQueryStringService _queryStringService;

        /// <summary>
        /// Default constructor with logging injected
        /// </summary>
        /// <param name="viewOperations">Access view data</param>
        /// <param name="loggingService">Dependency injected logging for all end points</param>
        /// <param name="queryStringService">Injected service to read http request information</param>
        public ViewsController(IViewOperations viewOperations, ILoggingService loggingService, IRequestQueryStringService queryStringService)
        {
            Check.NotNull(loggingService, nameof(loggingService));
            Check.NotNull(viewOperations, nameof(viewOperations));

            _loggingService = loggingService;
            _viewOperations = viewOperations;
            _queryStringService = queryStringService;
        }
        /// <summary>
        /// Retrieve data from a given view
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="viewName">Name of the view in the database.</param>
        [Route("")]
        [SwaggerResponse(200, "List of values from the view", typeof(IQueryable<SwaggerExampleType>))]
        [HttpGet]
        [UseOdataInSwagger]
        public IQueryable<object> Get(string connectorName, string viewName)
        {
            var (list, recordLimit) = _viewOperations.GetAllAndRecordLimit(connectorName, viewName, _loggingService);
            return list.ApplyQuery(recordLimit, _queryStringService.GetQueryStrings());
        }
    }
}