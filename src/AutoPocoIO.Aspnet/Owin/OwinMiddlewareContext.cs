using AutoPocoIO.Middleware;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AutoPocoIO.Owin
{
    public class OwinMiddlewareContext : IMiddlewareContext
    {

        public OwinMiddlewareContext(IDictionary<string, object> environment, IServiceProvider internalProvider)
        {
            Environment = environment;
            Request = new OwinMiddlewareRequest(environment);
            Response = new OwinMiddlewareResponse(environment);
            InternalServiceProvider = internalProvider;
        }

        public IDictionary<string, object> Environment { get; }

        public IMiddlewareRequest Request { get; protected set; }

        public IMiddlewareResponse Response { get; protected set; }

        public Match UriMatch { get; set; }
        public Uri RequestUri { get; set; }
        public Dictionary<string, string> QueryStrings { get; set; }


        public IServiceProvider InternalServiceProvider { get; protected set; }
    }
}