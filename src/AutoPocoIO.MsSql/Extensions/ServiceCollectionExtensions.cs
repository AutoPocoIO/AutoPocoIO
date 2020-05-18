using AutoPocoIO.MsSql.DynamicSchema.Db;
using AutoPocoIO.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AutoPocoIO.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection WithSqlServerResources(this IServiceCollection services)
        {
            services.AddTransient<IOperationResource, MsSqlResource>();
            services.AddTransient<IConnectionStringBuilder, MsSqlConnectionBuilder>();
            return services;
        }

        /// <summary>
        /// Default SqlServer provider configuration.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection ConfigureSqlServerApplicationDatabase(this IServiceCollection services, string connectionString)
        {
            services.ConfigureApplicationDatabase(c => c.UseSqlServer(connectionString, d => d.MigrationsHistoryTable("__MigrationsHistory", "AutoPoco")));
            return services;
        }

    }
}
