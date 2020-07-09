using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AutoPocoIO.Middleware
{
    /// <summary>
    ///  Encapsulates all HTTP-specific information about an individual HTTP request.
    /// </summary>
    public class AspNetCoreMiddlewareContext : IMiddlewareContext
    {

        /// <summary>
        /// Initialize for each http request
        /// </summary>
        /// <param name="httpContext">Http request context</param>
        /// <param name="internalProvider">Scoped middleware service provider</param>
        public AspNetCoreMiddlewareContext(HttpContext httpContext, IServiceProvider internalProvider)
            : base()
        {
            Request = new AspNetCoreMiddlewareRequest(httpContext);
            Response = new AspNetCoreMiddlewareResponse(httpContext);
            InternalServiceProvider = internalProvider;
            QueryStrings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        public IMiddlewareRequest Request { get; }
        /// <inheritdoc/>
        public IMiddlewareResponse Response { get;  }
        /// <inheritdoc/>
        public Match UriMatch { get; set; }
        /// <inheritdoc/>
        public Uri RequestUri { get; set; }
        /// <inheritdoc/>
        public IDictionary<string, string> QueryStrings { get; }
        /// <inheritdoc/>
        public IServiceProvider InternalServiceProvider { get;  }
    }
}