using AutoPocoIO.Constants;
using AutoPocoIO.Exceptions;
using AutoPocoIO.Middleware.Dispatchers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace AutoPocoIO.Middleware
{
    public class RouteCollection
    {
        private readonly List<Tuple<string, HttpMethodType, IMiddlewareDispatcher>> _dispatchers = new List<Tuple<string, HttpMethodType, IMiddlewareDispatcher>>();

        public void Add(string pathTemplate, HttpMethodType method, IMiddlewareDispatcher dispatcher)
        {
            _dispatchers.Add(new Tuple<string, HttpMethodType, IMiddlewareDispatcher>(pathTemplate, method, dispatcher));
        }

        public Tuple<IMiddlewareDispatcher, Match> FindDispatcher(IMiddlewareContext context, string path)
        {
            Check.NotNull(context, nameof(context));
            Check.NotNull(path, nameof(path));

            if (path.Length == 0)
                path = "/";
            else if (path.Length > 1)
                path = path.TrimEnd('/');

            bool found = false;
            foreach (var dispatcher in _dispatchers)
            {
                var pattern = dispatcher.Item1;
                if (!pattern.StartsWith("^", StringComparison.OrdinalIgnoreCase))
                    pattern = "^" + pattern;
                if (!pattern.EndsWith("$", StringComparison.OrdinalIgnoreCase))
                    pattern += "$";

                var match = Regex.Match(path, pattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline);

                if (match.Success)
                {
                    found = true;
                    if (context.Request.Method.Equals(dispatcher.Item2.ToString(), StringComparison.OrdinalIgnoreCase))
                        return new Tuple<IMiddlewareDispatcher, Match>(dispatcher.Item3, match);
                }
            }

            //Found but wrong http method
            if (found)
                context.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;

            return null;

        }
    }
}
