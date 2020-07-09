using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AutoPocoIO.Middleware
{
    /// <summary>
    /// Represents the incoming side of an individual HTTP request.
    /// </summary>
    public interface IMiddlewareRequest
    {
        /// <summary>
        /// Gets the HTTP method.
        /// </summary>
        string Method { get; }
        /// <summary>
        ///  Gets the request path from RequestPath.
        /// </summary>
        string Path { get; }
        /// <summary>
        ///  Gets the RequestPathBase.
        /// </summary>
        string PathBase { get; }
        /// <summary>
        /// Gets the request local IP address
        /// </summary>
        string LocalIpAddress { get; }
        /// <summary>
        /// Gets the request remote IP address
        /// </summary>
        string RemoteIpAddress { get; }
        /// <summary>
        ///  Gets the query value collection parsed from Request QueryString
        /// </summary>
        /// <param name="key">The query string key to search for</param>
        /// <returns></returns>
        string GetQuery(string key);
        /// <summary>
        /// Reads the request body if it is a form.
        /// </summary>
        /// <returns>Key/Value pair of the form values</returns>
        Task<IDictionary<string, string[]>> ReadFormAsync();
        /// <summary>
        ///Returns the combined components of the request URL in a fully escaped form suitable
        ///for use in HTTP headers and other HTTP operations.
        /// </summary>
        Uri RequestUri { get; }
        /// <summary>
        ///  Gets the RequestBody Stream.
        /// </summary>
        Stream Body { get; }
    }
}
