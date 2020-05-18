using AutoPocoIO.Middleware.Dispatchers;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AutoPocoIO.Middleware
{
    internal class RouteCollection
    {
        private readonly List<Tuple<string, IMiddlewareDispatcher>> _dispatchers = new List<Tuple<string, IMiddlewareDispatcher>>();

        public void Add(string pathTemplate, IMiddlewareDispatcher dispatcher)
        {
            _dispatchers.Add(new Tuple<string, IMiddlewareDispatcher>(pathTemplate, dispatcher));
        }

        public Tuple<IMiddlewareDispatcher, Match> FindDispatcher(string path)
        {
            if (path.Length == 0)
                path = "/";
            else if (path.Length > 1)
                path = path.TrimEnd('/');

            foreach (var dispatcher in _dispatchers)
            {
                var pattern = dispatcher.Item1;
                if (!pattern.StartsWith("^", StringComparison.OrdinalIgnoreCase))
                    pattern = "^" + pattern;
                if (!pattern.EndsWith("$", StringComparison.OrdinalIgnoreCase))
                    pattern += "$";

                var match = Regex.Match(path, pattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline);

                if (match.Success)
                    return new Tuple<IMiddlewareDispatcher, Match>(dispatcher.Item2, match);
            }

            return null;

        }
    }
}
