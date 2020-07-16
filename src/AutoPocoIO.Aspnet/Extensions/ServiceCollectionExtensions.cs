using AutoPoco.DependencyInjection;
using AutoPocoIO.Exceptions;
using AutoPocoIO.LoggingMiddleware;
using AutoPocoIO.Owin;
using AutoPocoIO.WebApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;
using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Mvc;

namespace AutoPocoIO.Extensions
{
    public static partial class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the minimum essential AutoPoco services to the specified <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/>
        /// </summary>
        /// <param name="services"> The <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/> to add services to.</param>
        /// <returns></returns>
        public static IServiceCollection AddAutoPoco(this IServiceCollection services)
        {
            return services
                .AddDatabaseOperations()
                .AddLogging()
                .AddDashboard()
                .AddAutoPocoWebApiEndPoints()
                .AddDI();

        }

        /// <summary>
        /// Register all WebApi and MVC controllers types.
        /// </summary>
        /// <param name="services"> The <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/> to add services to.</param>
        /// <param name="assembly">Application assembly to search for controllers</param>
        /// <returns></returns>
        public static IServiceCollection RegisterControllers(this IServiceCollection services, Assembly assembly)
        {
            Check.NotNull(assembly, nameof(assembly));

            var controllers = assembly.GetTypes()
                .Where(c => (typeof(IController).IsAssignableFrom(c) || typeof(IHttpController).IsAssignableFrom(c)) &&
                            c.Name.EndsWith("Controller", StringComparison.Ordinal));

            foreach (var controller in controllers)
                services.TryAddTransient(controller);

            return services;
        }

        /// <summary>
        /// Registers default <see cref="System.Web.Http.ApiController"/> with minimum AutoPoco services
        /// </summary>
        /// <param name="services"> The <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/> to add services to.</param>
        /// <returns></returns>
        private static IServiceCollection AddAutoPocoWebApiEndPoints(this IServiceCollection services)
        {
            services.AddTransient<SchemaController>();
            services.AddTransient<StoredProcedureController>();
            services.AddTransient<StoredProcedureDefinitionController>();
            services.AddTransient<TableDefinitionController>();
            services.AddTransient<TablesController>();
            services.AddTransient<ViewsController>();

            return services;
        }


        private static IServiceCollection AddLogging(this IServiceCollection services)
        {
            services.AddTransient<LogRequestAndResponseMiddleware>();
            return services;
        }

        private static IServiceCollection AddDashboard(this IServiceCollection services)
        {
            services.AddTransient<DashboardMiddleware>();
            return services;
        }


        private static IServiceCollection AddDI(this IServiceCollection services)
        {
            services.TryAddTransient<IServiceScope, ServiceScope>();
            services.TryAddTransient<IServiceScopeFactory, ServiceScopeFactory>();
            services.TryAddScoped<IServiceProvider>(c => c);
            return services;
        }

    }
}
