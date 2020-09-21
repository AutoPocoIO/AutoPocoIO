using AutoPocoIO.Constants;
using AutoPocoIO.Dashboard;
using AutoPocoIO.Exceptions;
using AutoPocoIO.LoggingMiddleware;
using AutoPocoIO.Models;
using AutoPocoIO.Services;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace AutoPocoIO.Extensions
{
    /// <summary>
    /// AspNet Core Extenstions to set up the dashboard 
    /// </summary>
    public static class AppBuilderExtensions
    {
        /// <summary>
        /// Default dashboard set up for with dash prefix "/autopoco"
        /// </summary>
        /// <param name="builder">The builder being used to configure the context.</param>
        /// <returns>The options builder so that further configuration can be chained.</returns>
        public static IApplicationBuilder UseAutoPoco(this IApplicationBuilder builder)
        {
            return builder.UseAutoPoco(new AutoPocoOptions());
        }

        /// <summary>
        /// Set up the dashboard
        /// </summary>
        /// <param name="builder">The builder being used to configure the context.</param>
        /// <param name="options">Dashboard setup options</param>
        /// <returns>The options builder so that further configuration can be chained.</returns>

        public static IApplicationBuilder UseAutoPoco(this IApplicationBuilder builder, AutoPocoOptions options)
        {
            Check.NotNull(options, nameof(options));
            Check.NotNull(builder, nameof(builder));

            if (options.DashboardPath.Length <= 1 || options.DashboardPath[0] != '/')
                throw new ArgumentException(ExceptionMessages.MiddlewarePath, nameof(options));

            //Set dashboard path prefix for api routes
            AutoPocoConfiguration.DashboardPathPrefix = options.DashboardPath.Trim('/');
            string dashPath = "/" + AutoPocoConfiguration.DashboardPathPrefix;

#if NETCORE2_2
           // builder.UseEndpointRouting();
#else
            builder.UseRouting();
#endif 

            builder.UseSwagger(SwaggerConfig.SwaggerAppBuilderFunc(dashPath));
            builder.UseSwaggerUI(SwaggerConfig.SwaggerUIAppBuilderFunc(dashPath));

            //Log All Api Request
            builder.UseMiddleware<LogRequestAndResponseMiddleware>();

       

            //Map Dashboard routes
            if (options.UseDashboard)
                builder.UseMiddlewareWhen<AspNetCoreDashboardMiddleware>(dashPath);

            //Enable OData
#if NETCORE2_2
            builder.UseMvc(routeBuilder =>
            {
                routeBuilder.EnableDependencyInjection();
                routeBuilder.Count().Filter().OrderBy().Expand().Select().MaxTop(1000);
            });

#else
             builder.UseEndpoints(routeBuilder =>
            {
                routeBuilder.MapControllers();
                routeBuilder.EnableDependencyInjection();
                routeBuilder.Count().Filter().OrderBy().Expand().Select().MaxTop(1000);
            });
#endif

            var scopeFactory = builder.ApplicationServices.GetRequiredService<IServiceScopeFactory>();
            using (var scope = scopeFactory.CreateScope())
            {
                var provider = scope.ServiceProvider;
                var dbSetupService = provider.GetRequiredService<IAppDatabaseSetupService>();

                if (!string.IsNullOrEmpty(options.SaltVector) || !string.IsNullOrEmpty(options.SecretKey))
                    dbSetupService.SetupEncryption(options.SaltVector, options.SecretKey, options.CacheTimeoutMinutes);

                dbSetupService.Migrate();
            }

            return builder;
        }

        /// <summary>
        /// Conditionally creates a branch in the request pipeline that is rejoined to the main pipeline.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="pathMatch"></param>
        /// <param name="args">Additional middleware consturctor values</param>
        /// <returns></returns>
        public static IApplicationBuilder UseMiddlewareWhen<TMiddleware>(this IApplicationBuilder builder, PathString pathMatch, params object[] args)
        {
            return builder.UseWhen(
                context => context.Request.Path.StartsWithSegments(pathMatch, StringComparison.OrdinalIgnoreCase),
                c => c.UseMiddleware<TMiddleware>(args));
        }
    }
}
