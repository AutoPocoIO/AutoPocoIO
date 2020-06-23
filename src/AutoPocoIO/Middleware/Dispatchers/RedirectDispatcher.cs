using AutoPocoIO.Exceptions;
using AutoPocoIO.Services;
using System.Threading.Tasks;

namespace AutoPocoIO.Middleware.Dispatchers
{
    public class RedirectDispatcher : IMiddlewareDispatcher
    {
        private string _location;

        public RedirectDispatcher(string location)
        {
            _location = location;
        }

        public Task Dispatch(IMiddlewareContext context, ILoggingService loggingService)
        {
            Check.NotNull(context, nameof(context));
            Check.NotNull(loggingService, nameof(loggingService));

            _location = $"/{context.Request.PathBase.Trim('/')}/{_location.Trim('/')}";

            context.Response.Redirect(_location);
            return Task.CompletedTask;
        }
    }
}
