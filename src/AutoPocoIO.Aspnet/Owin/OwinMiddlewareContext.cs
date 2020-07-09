using AutoPocoIO.Middleware;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AutoPocoIO.Owin
{
    /// <summary>
    /// Encapsulates all HTTP-specific information about an individual HTTP request.
    /// </summary>
    public class OwinMiddlewareContext : IMiddlewareContext
    {
        /// <summary>
        /// Initialize for each http request
        /// </summary>
        /// <param name="environment">Owin environment</param>
        /// <param name="internalProvider">Scoped middleware service provider</param>
        public OwinMiddlewareContext(IDictionary<string, object> environment, IServiceProvider internalProvider)
        {
            Environment = environment;
            Request = new OwinMiddlewareRequest(environment);
            Response = new OwinMiddlewareResponse(environment);
            InternalServiceProvider = internalProvider;
            QueryStrings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Owin middleware environment variables
        /// </summary>
        public IDictionary<string, object> Environment { get; }

        ///<inheritdoc/>
        public IMiddlewareRequest Request { get; protected set; }
        ///<inheritdoc/>
        public IMiddlewareResponse Response { get; protected set; }
        ///<inheritdoc/>
        public Match UriMatch { get; set; }
        ///<inheritdoc/>
        public Uri RequestUri { get; set; }
        ///<inheritdoc/>
        public IDictionary<string, string> QueryStrings { get; }

        ///<inheritdoc/>
        public IServiceProvider InternalServiceProvider { get; protected set; }
    }
}