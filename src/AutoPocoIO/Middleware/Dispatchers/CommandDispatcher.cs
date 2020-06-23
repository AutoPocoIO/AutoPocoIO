using AutoPocoIO.Exceptions;
using AutoPocoIO.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AutoPocoIO.Middleware.Dispatchers
{
    public class CommandDispatcher<TPage> : IMiddlewareDispatcher where TPage : RazorPage
    {
        private readonly Action<TPage, Match> _command;
        public CommandDispatcher(Action<TPage, Match> command)
        {
            _command = command;
        }

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
