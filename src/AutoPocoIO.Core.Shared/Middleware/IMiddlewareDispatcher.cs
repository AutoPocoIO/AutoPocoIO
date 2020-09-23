using AutoPocoIO.Services;
using System.Threading.Tasks;

namespace AutoPocoIO.Middleware.Dispatchers
{
    /// <summary>
    /// Handle middleware request.
    /// </summary>
    public interface IMiddlewareDispatcher
    {
        /// <summary>
        /// Set up response for middleware request.
        /// </summary>
        /// <param name="context">Request context.</param>
        /// <param name="loggingService">Request scoped logger.</param>
        /// <returns></returns>
        Task Dispatch(IMiddlewareContext context, ILoggingService loggingService);
    }
}
