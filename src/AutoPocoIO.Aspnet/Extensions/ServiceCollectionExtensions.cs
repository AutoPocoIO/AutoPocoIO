using AutoPoco.DependencyInjection;
using AutoPocoIO.LoggingMiddleware;
using AutoPocoIO.Owin;
using AutoPocoIO.WebApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

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
