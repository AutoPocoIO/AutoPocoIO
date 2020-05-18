using AutoPocoIO.LoggingMiddleware;
using AutoPocoIO.Owin;
using AutoPocoIO.WebApi;
using Microsoft.Extensions.DependencyInjection;

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
                .AddAutoPocoWebApiEndPoints();
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


    }
}
