using AutoPocoIO.LoggingMiddleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoPocoIO.Extensions
{
    /// <summary>
    /// Http Route extensions
    /// </summary>
    public static class RouteExtensions
    {
        /// <summary>
        /// Gets HttpRequest query string values
        /// </summary>
        /// <param name="request">Current Http Request</param>
        /// <returns>Key/Value pair of query strings</returns>
        public static Dictionary<string, string> GetQueryStrings(this HttpRequest request)
        {
            if (request == null)
                return new Dictionary<string, string>();
            else
                return request.Query.ToDictionary(kv => kv.Key, kv => kv.Value.ToString(),
                               StringComparer.OrdinalIgnoreCase);
        }
    }
}