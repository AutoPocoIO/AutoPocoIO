using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AutoPocoIO.Middleware
{
    public interface IMiddlewareContext 
    {
        IMiddlewareRequest Request { get; }
        IMiddlewareResponse Response { get; }

        Match UriMatch { get; set; }
        Uri RequestUri { get; set; }

        IDictionary<string, string> QueryStrings { get; }

      
        IServiceProvider InternalServiceProvider { get; }
    }
}