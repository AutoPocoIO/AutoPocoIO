using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AutoPocoIO.Middleware
{
    public class AspNetCoreMiddlewareContext : IMiddlewareContext
    {

        public AspNetCoreMiddlewareContext(HttpContext httpContext, IServiceProvider internalProvider)
            : base()
        {
            Request = new AspNetCoreMiddlewareRequest(httpContext);
            Response = new AspNetCoreMiddlewareResponse(httpContext);
            InternalServiceProvider = internalProvider;
            QueryStrings = new Dictionary<string, string>();
        }

        public IMiddlewareRequest Request { get; }

        public IMiddlewareResponse Response { get;  }

        public Match UriMatch { get; set; }
        public Uri RequestUri { get; set; }
        public IDictionary<string, string> QueryStrings { get; }

        public IServiceProvider InternalServiceProvider { get;  }
    }
}