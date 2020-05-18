using AutoPocoIO.Constants;
using AutoPocoIO.Exceptions;
using AutoPocoIO.Models;
using AutoPocoIO.Owin;
using AutoPocoIO.Services;
using AutoPocoIO.SwaggerAddons;
using Microsoft.AspNet.OData.Extensions;
using Owin;
using System;
using System.Linq;
using System.Web.Http;

namespace AutoPocoIO.Extensions
{
    /// <summary>
    /// Owin set up dashboard
    /// </summary>
    public static partial class AppBuilderExtensions
    {
        /// <summary>
        /// Default dashboard set up for with default settings
        /// </summary>
        /// <param name="builder">The builder being used to configure the context.</param>
        /// <param name="config">Current Httpconfiguration</param>
        /// <returns>The options builder so that further configuration can be chained.</returns>
        public static IAppBuilder UseAutoPoco(this IAppBuilder builder, HttpConfiguration config)
        {
            return builder.UseAutoPoco(config, new AutoPocoOptions());
        }


        /// <summary>
        /// Set up dashboard with a basic license
        /// </summary>
        /// <param name="builder">The builder being used to configure the context.</param>
        /// <param name="options">Dashboard setup options</param>
        /// <param name="config">Current Httpconfiguration</param>
        /// <returns>The options builder so that further configuration can be chained.</returns>

        public static IAppBuilder UseAutoPoco(this IAppBuilder builder, HttpConfiguration config, AutoPocoOptions options)
        {
            Check.NotNull(options, nameof(options));
            Check.NotNull(config, nameof(config));

            if (options.DashboardPath.Length <= 1 || options.DashboardPath[0] != '/')
                throw new ArgumentException(ExceptionMessages.MiddlewarePath, nameof(options));

            //Set dashboard path prefix for api routes
            AutoPocoConfiguration.DashboardPathPrefix = options.DashboardPath.Trim('/');

            config.MapHttpAttributeRoutes();
            SwaggerConfig.Register(config, options.DashboardPath);

            //Common items
            config.EnableDependencyInjection();
            config.Count().Filter().OrderBy().Expand().Select().MaxTop(1000);

            builder.UseWithDependencyInjection<LoggingMiddleware.LogRequestAndResponseMiddleware>(config);

            if (options.UseDashboard)
                builder.UseWithDependencyInjection<DashboardMiddleware>(config);

            //Migrate and set up encrption
            var dbSetupService = config.DependencyResolver.GetRequiredService<IAppDatabaseSetupService>();

            if (!string.IsNullOrEmpty(options.SaltVector) || !string.IsNullOrEmpty(options.SecretKey))
                dbSetupService.SetupEncryption(options.SaltVector, options.SecretKey, options.CacheTimeoutMinutes);

            dbSetupService.Migrate();

            return builder;
        }

        internal static IAppBuilder UseWithDependencyInjection<T>(this IAppBuilder app, HttpConfiguration config) where T : class, IOwinMiddlewareWithDI
        {
            return app.Use<OwinContainerWrapper<T>>(config);
        }
    }
}