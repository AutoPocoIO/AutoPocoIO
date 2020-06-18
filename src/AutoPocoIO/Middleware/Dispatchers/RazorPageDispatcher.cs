using AutoPocoIO.Services;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AutoPocoIO.Middleware.Dispatchers
{
    public class RazorPageDispatcher : IMiddlewareDispatcher
    {
        private readonly Func<Match, RazorPage> _pageFunc;

        public RazorPageDispatcher(Func<Match, RazorPage> pageFunc)
        {
            _pageFunc = pageFunc;
        }

        public Task Dispatch(IMiddlewareContext context, ILoggingService loggingService)
        {
            context.Response.ContentType = "text/html";
            var page = _pageFunc(context.UriMatch);
            page.LoggingService = loggingService;
            page.Assign(context);

            return context.Response.WriteAsync(page.ToString());
        }
    }
}
