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
            _location = $"/{context.Request.PathBase.Trim('/')}/{_location.Trim('/')}";

            context.Response.Redirect(_location);
            return Task.CompletedTask;
        }
    }
}
