using AutoPocoIO.Exceptions;
using AutoPocoIO.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AutoPocoIO.Middleware.Dispatchers
{
    /// <summary>
    /// Return a page and run a function to get page data.
    /// </summary>
    /// <typeparam name="TPage">Page type to display.</typeparam>
    public class RazorPageDispatcher<TPage> : IMiddlewareDispatcher where TPage : RazorPage
    {
        private readonly Action<TPage, Match> _pageFunc;

        /// <summary>
        /// Initialize dispatcher a page.
        /// </summary>
        /// <param name="pageFunc">Page and function to display</param>
        public RazorPageDispatcher(Action<TPage, Match> pageFunc)
        {
            _pageFunc = pageFunc;
        }

        /// <summary>
        /// Run page function and parse page to response.
        /// </summary>
        ///<inheritdoc/>
        public Task Dispatch(IMiddlewareContext context, ILoggingService loggingService)
        {
            Check.NotNull(context, nameof(context));
            Check.NotNull(loggingService, nameof(loggingService));

            context.Response.ContentType = "text/html";

            TPage page = context.InternalServiceProvider.GetRequiredService<TPage>();
            page.LoggingService = loggingService;
            page.Assign(context);
            _pageFunc(page, context.UriMatch);


            return context.Response.WriteAsync(page.ToString());
        }
    }
}
