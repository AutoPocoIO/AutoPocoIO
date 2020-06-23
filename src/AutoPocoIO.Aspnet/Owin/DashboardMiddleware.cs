using AutoPocoIO.Dashboard;
using AutoPocoIO.Exceptions;
using AutoPocoIO.Extensions;
using AutoPocoIO.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin;
using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AutoPocoIO.Owin
{
    public class DashboardMiddleware : IOwinMiddlewareWithDI
    {
        private readonly ILoggingService _loggingService;
        private readonly string _basePath;
        private readonly DashboardRoutes _routes;
        private readonly IServiceProvider _serviceProvider;

        public DashboardMiddleware(IServiceProvider provider, ILoggingService loggingService)
        {
            _basePath = "/" + AutoPocoConfiguration.DashboardPathPrefix;
            _loggingService = loggingService;

            var _serviceScope = DashboardServiceProvider.Instance.GetServiceProvider(provider)
                 .GetRequiredService<IServiceScopeFactory>()
                 .CreateScope();

            var scopedServiceProvider = _serviceScope.ServiceProvider;

            _routes = scopedServiceProvider.GetRequiredService<DashboardRoutes>();
            _serviceProvider = scopedServiceProvider;

        }

        public OwinMiddleware NextComponent { get; set; }

        public async Task Invoke(IOwinContext context)
        {
            Check.NotNull(context, nameof(context));

            //Only try if at basepath
            if (context.Request.Path.StartsWithSegments(new PathString(_basePath)) &&
                    !context.Request.Path.StartsWithSegments(new PathString(_basePath + "/swagger")) &&
                    !context.Request.Path.StartsWithSegments(new PathString(_basePath + "/api")))
            {
                var env = context.Environment;
                if (!env["owin.RequestPathBase"].ToString().Equals(_basePath, StringComparison.InvariantCultureIgnoreCase))
                {
                    env["owin.RequestPathBase"] = _basePath;
                    env["owin.RequestPath"] = env["owin.RequestPath"].ToString().Replace(_basePath, "");
                }

                var dashContext = new OwinMiddlewareContext(env, _serviceProvider);

                var findResult = _routes.Routes.FindDispatcher(dashContext, context.Request.Path.Value);

                if (context.Response.StatusCode == (int)HttpStatusCode.MethodNotAllowed)
                    return;

                if (findResult == null)
                {
                    await NextComponent.Invoke(context).ConfigureAwait(false);
                    return;
                }

                dashContext.UriMatch = findResult.Item2;
                dashContext.RequestUri = context.Request.Uri;
                dashContext.QueryStrings.Clear();

                var queryStrings = Regex.Matches(context.Request.QueryString.Value, "([^?=&]+)(=([^&]*))?")
                    .Cast<Match>();
                foreach (var querystring in queryStrings)
                    dashContext.QueryStrings[querystring.Groups[1].Value] = querystring.Groups[3].Value;

                await findResult.Item1.Dispatch(dashContext, _loggingService).ConfigureAwait(false);
            }
            else
                await NextComponent.Invoke(context).ConfigureAwait(false);
        }
    }
}
