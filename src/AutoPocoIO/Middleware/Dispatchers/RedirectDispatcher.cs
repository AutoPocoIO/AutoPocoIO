using AutoPocoIO.Exceptions;
using AutoPocoIO.Services;
using System.Threading.Tasks;

namespace AutoPocoIO.Middleware.Dispatchers
{
    public class RedirectDispatcher : IMiddlewareDispatcher
    {
        private string _location;
        private readonly bool _useBasePath;

        public RedirectDispatcher(string location, bool useBasePath = true)
        {
            _location = location;
            _useBasePath = useBasePath;
        }

        public Task Dispatch(IMiddlewareContext context, ILoggingService loggingService)
        {
            Check.NotNull(context, nameof(context));
            Check.NotNull(loggingService, nameof(loggingService));

            if (_useBasePath)
                _location = $"/{context.Request.PathBase.Trim('/')}/{_location.Trim('/')}";

            context.Response.Redirect(_location);
            return Task.CompletedTask;
        }
    }
}
