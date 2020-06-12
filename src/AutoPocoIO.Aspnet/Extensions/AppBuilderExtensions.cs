using AutoPoco.DependencyInjection;
using AutoPocoIO.Constants;
using AutoPocoIO.Exceptions;
using AutoPocoIO.Models;
using AutoPocoIO.Owin;
using AutoPocoIO.Services;
using AutoPocoIO.SwaggerAddons;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Mvc;

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
        /// <returns>The options builder so that further configuration can be chained.</returns>
        public static IAppBuilder UseAutoPoco(this IAppBuilder builder, IEnumerable<ServiceDescriptor> descriptors)
        {
            return builder.UseAutoPoco(new HttpConfiguration(), new AutoPocoOptions(), descriptors);
        }


        /// <summary>
        /// Default dashboard set up for with default settings
        /// </summary>
        /// <param name="builder">The builder being used to configure the context.</param>
        ///  /// <param name="options">Dashboard setup options</param>
        /// <returns>The options builder so that further configuration can be chained.</returns>
        public static IAppBuilder UseAutoPoco(this IAppBuilder builder, AutoPocoOptions options, IEnumerable<ServiceDescriptor> descriptors)
        {
            return builder.UseAutoPoco(new HttpConfiguration(), options, descriptors);
        }

        /// <summary>
        /// Default dashboard set up for with default settings
        /// </summary>
        /// <param name="builder">The builder being used to configure the context.</param>
        /// <param name="config">Current Httpconfiguration</param>
        /// <returns>The options builder so that further configuration can be chained.</returns>
        public static IAppBuilder UseAutoPoco(this IAppBuilder builder, HttpConfiguration config, IEnumerable<ServiceDescriptor> descriptors)
        {
            return builder.UseAutoPoco(config, new AutoPocoOptions(), descriptors);
        }


        /// <summary>
        /// Set up dashboard with a basic license
        /// </summary>
        /// <param name="builder">The builder being used to configure the context.</param>
        /// <param name="options">Dashboard setup options</param>
        /// <param name="config">Current Httpconfiguration</param>
        /// <returns>The options builder so that further configuration can be chained.</returns>

        public static IAppBuilder UseAutoPoco(this IAppBuilder builder, HttpConfiguration config, AutoPocoOptions options, IEnumerable<ServiceDescriptor> services)
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

            //DI
            var container = new Container(services);
            //Check if assigned (Empty Resolver is internal)
            if (config.DependencyResolver.GetType().FullName == "System.Web.Http.Dependencies.EmptyResolver")
            {
                config.DependencyResolver = new AutoPocoDependencyResolver(container);
                config.MessageHandlers.Insert(0, new RequestScopeFromOwinHandler());
            }
            if(DependencyResolver.Current.GetType().FullName == "System.Web.Mvc.DependencyResolver+DefaultDependencyResolver")
                DependencyResolver.SetResolver(new AutoPocoDependencyResolver(container));

            builder.Use<ContainerMiddleware>(config);
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