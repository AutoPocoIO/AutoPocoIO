using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AutoPocoIO.Middleware
{
    internal interface IMiddlewareContext 
    {
        IMiddlewareRequest Request { get; }
        IMiddlewareResponse Response { get; }

        Match UriMatch { get; set; }
        Uri RequestUri { get; set; }

        Dictionary<string, string> QueryStrings { get; set; }

      
        IServiceProvider InternalServiceProvider { get; }
    }
}