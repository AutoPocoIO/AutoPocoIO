using AutoPocoIO.Services;
using System.Threading.Tasks;

namespace AutoPocoIO.Middleware.Dispatchers
{
    public interface IMiddlewareDispatcher
    {
        Task Dispatch(IMiddlewareContext context, ILoggingService loggingService);
    }
}
