using AutoPocoIO.Exceptions;
using AutoPocoIO.Services;
using System.Threading.Tasks;

namespace AutoPocoIO.Middleware.Dispatchers
{
    /// <summary>
    /// Return razor page.
    /// </summary>
    public class RazorPageDispatcher : IMiddlewareDispatcher
    {
        private readonly RazorPage _page;

        /// <summary>
        /// Initialize dispatcher a page.
        /// </summary>
        /// <param name="page">Page to display</param>
        public RazorPageDispatcher(RazorPage page)
        {
            _page = page;
        }

        /// <summary>
        /// Parse page to response.
        /// </summary>
        ///<inheritdoc/>
        public Task Dispatch(IMiddlewareContext context, ILoggingService loggingService)
        {
            Check.NotNull(context, nameof(context));
            Check.NotNull(loggingService, nameof(loggingService));

            context.Response.ContentType = "text/html";
            _page.LoggingService = loggingService;
            _page.Assign(context);
            return context.Response.WriteAsync(_page.ToString());
        }
    }
}
