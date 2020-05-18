using AutoPocoIO.Services;
using System.Threading.Tasks;

namespace AutoPocoIO.Middleware.Dispatchers
{
    internal interface IMiddlewareDispatcher
    {
        Task Dispatch(IMiddlewareContext context, ILoggingService loggingService);
    }
}
