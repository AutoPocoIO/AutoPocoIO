using AutoPocoIO.Exceptions;
using AutoPocoIO.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AutoPocoIO.Middleware.Dispatchers
{
    /// <summary>
    /// Run action on http request.
    /// </summary>
    /// <typeparam name="TPage">Razor page to run action against.</typeparam>
    public class CommandDispatcher<TPage> : IMiddlewareDispatcher where TPage : RazorPage
    {
        private readonly Action<TPage, Match> _command;
        /// <summary>
        /// Initialize dispatcher with an action.
        /// </summary>
        /// <param name="command">Action to run.</param>
        public CommandDispatcher(Action<TPage, Match> command)
        {
            _command = command;
        }

        /// <summary>
        /// Run action if request is a POST
        /// </summary>
        ///<inheritdoc/>
        public Task Dispatch(IMiddlewareContext context, ILoggingService loggingService)
        {
            Check.NotNull(context, nameof(context));
            Check.NotNull(loggingService, nameof(loggingService));

            var request = context.Request;
            var response = context.Response;

            if (!"POST".Equals(request.Method, StringComparison.OrdinalIgnoreCase))
            {
                response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                return Task.FromResult(false);
            }

            TPage page = context.InternalServiceProvider.GetRequiredService<TPage>();
            page.LoggingService = loggingService;
            _command(page, context.UriMatch);
            response.StatusCode = (int)HttpStatusCode.OK;
            return Task.FromResult(true);
        }
    }
}
