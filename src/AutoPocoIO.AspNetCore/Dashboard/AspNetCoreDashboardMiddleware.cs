using AutoPocoIO.Exceptions;
using AutoPocoIO.Extensions;
using AutoPocoIO.Middleware;
using AutoPocoIO.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Threading.Tasks;

namespace AutoPocoIO.Dashboard
{
    /// <summary>
    /// Middleware to show the AutoPocoIO dashbard.
    /// </summary>
    public class AspNetCoreDashboardMiddleware
    {
        private readonly RequestDelegate _next;
        private DashboardRoutes _routes;
        private IServiceProvider _serviceProvider;

        /// <summary>
        /// Use Middleware.
        /// </summary>
        /// <param name="next">Next middleware in the pipeline.</param>
        public AspNetCoreDashboardMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Find dispatcher for the dashboard page requested 
        /// </summary>
        /// <param name="httpContext">Current HttpContext of the request</param>
        /// <param name="provider">Application service profider</param>
        /// <param name="loggingService">AutoPocoIO resource operation logging service</param>
        /// <returns></returns>
        public async Task Invoke(HttpContext httpContext, IServiceProvider provider, ILoggingService loggingService)
        {
            Check.NotNull(httpContext, nameof(httpContext));
            Check.NotNull(provider, nameof(provider));
            Check.NotNull(loggingService, nameof(loggingService));


            SetupServices(provider);

            var context = new AspNetCoreMiddlewareContext(httpContext, _serviceProvider);

            string routeToSearch = httpContext.Request.Path.Value;
            string basePath = "/" + AutoPocoConfiguration.DashboardPathPrefix;

            if (!httpContext.Request.PathBase.ToString().Equals(basePath, StringComparison.InvariantCultureIgnoreCase))
                routeToSearch = httpContext.Request.Path.Value.Replace(basePath, "");

            var findResult = _routes.Routes.FindDispatcher(context, routeToSearch);


            if (context.Response.StatusCode == (int)HttpStatusCode.MethodNotAllowed)
                return;
            if (findResult == null)
            {
                await _next.Invoke(httpContext)
                           .ConfigureAwait(false);
                return;
            }


            context.UriMatch = findResult.Item2;

            UriBuilder uriBuilder = new UriBuilder
            {
                Scheme = httpContext.Request.Scheme,
                Host = httpContext.Request.Host.Host,
                Path = httpContext.Request.Path.ToString(),
                Query = httpContext.Request.QueryString.ToString()
            };
            context.RequestUri = uriBuilder.Uri;
            context.QueryStrings.Clear();
            foreach (var querystring in httpContext.Request.GetQueryStrings())
                context.QueryStrings.Add(querystring);

            httpContext.Request.PathBase = basePath;
            await findResult.Item1.Dispatch(context, loggingService)
                                  .ConfigureAwait(false);
        }

        private void SetupServices(IServiceProvider provider)
        {
            var _serviceScope = DashboardServiceProvider.Instance.GetServiceProvider(provider)
              .GetRequiredService<IServiceScopeFactory>()
              .CreateScope();

            var scopedServiceProvider = _serviceScope.ServiceProvider;

            _routes = scopedServiceProvider.GetRequiredService<DashboardRoutes>();
            _serviceProvider = scopedServiceProvider;

        }
    }
}