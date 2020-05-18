using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AutoPocoIO.Middleware
{
    internal sealed class AspNetCoreMiddlewareContext : IMiddlewareContext
    {

        public AspNetCoreMiddlewareContext(HttpContext httpContext, IServiceProvider internalProvider)
            : base()
        {
            Request = new AspNetCoreMiddlewareRequest(httpContext);
            Response = new AspNetCoreMiddlewareResponse(httpContext);
            InternalServiceProvider = internalProvider;
        }

        public IMiddlewareRequest Request { get; private set; }

        public IMiddlewareResponse Response { get; private set; }

        public Match UriMatch { get; set; }
        public Uri RequestUri { get; set; }
        public Dictionary<string, string> QueryStrings { get; set; }

        public IServiceProvider InternalServiceProvider { get; private set; }
    }
}