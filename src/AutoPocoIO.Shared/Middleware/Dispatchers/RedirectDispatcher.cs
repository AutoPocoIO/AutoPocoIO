using AutoPocoIO.Exceptions;
using AutoPocoIO.Services;
using System.Threading.Tasks;

namespace AutoPocoIO.Middleware.Dispatchers
{
    /// <summary>
    /// Redirect request.
    /// </summary>
    public class RedirectDispatcher : IMiddlewareDispatcher
    {
        private string _location;
        private readonly bool _useBasePath;

        /// <summary>
        /// Initialize dispatcher with a path to redirect browser.
        /// </summary>
        /// <param name="location">Redirect path.</param>
        /// <param name="useBasePath">If true prepend request base path.</param>
        public RedirectDispatcher(string location, bool useBasePath = true)
        {
            _location = location;
            _useBasePath = useBasePath;
        }

        /// <summary>
        /// Redirect browser to location.
        /// </summary>
        ///<inheritdoc/>
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
