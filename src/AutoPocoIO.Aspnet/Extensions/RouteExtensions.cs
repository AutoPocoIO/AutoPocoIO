using AutoPocoIO.LoggingMiddleware;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace AutoPocoIO.Extensions
{
    public static class RouteExtensions
    {
        public static Dictionary<string, string> GetQueryStrings(this HttpRequestMessage request)
        {
            if (request == null)
                return new Dictionary<string, string>();
            else
                return request.GetQueryNameValuePairs()
                          .ToDictionary(kv => kv.Key, kv => kv.Value,
                               StringComparer.OrdinalIgnoreCase);
        }

        public static Dictionary<string, string> GetQueryStrings(this HttpRequest request)
        {
            if (request == null)
            {
                return new Dictionary<string, string>();
            }
            else
            {
                return request.QueryString
                    .AllKeys
                    .ToDictionary(kv => kv, kv => request.QueryString[kv],
                               StringComparer.OrdinalIgnoreCase);
            }
        }

        internal static string DescriptionFromStatusCode(this ContextLogParameters logParameters, string statusCode)
        {
            var response = logParameters.Context.Response;
            return $"{response.StatusCode} : {(string.IsNullOrEmpty(statusCode) ? response.ReasonPhrase : statusCode)}";
        }

        internal static string GetIPFromLogParameters(this ContextLogParameters logParameters)
        {
           return logParameters.Context.Request.RemoteIpAddress.ToString(CultureInfo.InvariantCulture);
        }

    }
}