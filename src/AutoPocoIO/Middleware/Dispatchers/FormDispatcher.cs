using AutoPocoIO.Dashboard;
using AutoPocoIO.Middleware.Dispatchers;
using AutoPocoIO.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Threading.Tasks;

namespace AutoPocoIO.Middleware
{
    public class FormDispatcher<TPage> : IMiddlewareDispatcher where TPage : RazorPage, IRazorForm
    {

        public async Task Dispatch(IMiddlewareContext context, ILoggingService loggingService)
        {
            var request = context.Request;
            var response = context.Response;

            if (!"POST".Equals(request.Method, StringComparison.OrdinalIgnoreCase))
            {
                response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                return;
            }

            TPage page = context.InternalServiceProvider.GetRequiredService<TPage>();
            page.LoggingService = loggingService;

            var form = await context.Request.ReadFormAsync().ConfigureAwait(false);
            page.Assign(context);
            page.SetForm(form);

            IMiddlewareDispatcher result = page.Save();

            await result.Dispatch(context, loggingService).ConfigureAwait(false);
        }
    }
}
