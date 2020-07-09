using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AutoPocoIO.Middleware
{
    /// <summary>
    ///  Encapsulates all HTTP-specific information about an individual HTTP request.
    /// </summary>
    public interface IMiddlewareContext 
    {
        /// <summary>
        /// Http request information
        /// </summary>
        IMiddlewareRequest Request { get; }
        /// <summary>
        /// Http response information
        /// </summary>
        IMiddlewareResponse Response { get; }
        /// <summary>
        /// Regex match to dispatcher
        /// </summary>
        Match UriMatch { get; set; }
        /// <summary>
        /// Request uri
        /// </summary>
        Uri RequestUri { get; set; }
        /// <summary>
        /// Request query strings
        /// </summary>
        IDictionary<string, string> QueryStrings { get; }
        /// <summary>
        /// Scoped middleware service provider
        /// </summary>
        IServiceProvider InternalServiceProvider { get; }
    }
}