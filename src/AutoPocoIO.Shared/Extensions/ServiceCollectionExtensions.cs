using AutoPocoIO.Api;
using AutoPocoIO.Context;
using AutoPocoIO.Exceptions;
using AutoPocoIO.Factories;
using AutoPocoIO.Migrations;
using AutoPocoIO.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AutoPocoIO.Extensions
{
    /// <summary>
    /// Register services.
    /// </summary>
    public static partial class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the minimum essential AutoPoco services to the specified <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/>
        /// </summary>
        /// <param name="services"> The <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/> to add services to.</param>
        /// <param name="options">Events to fire before or after logging.</param>
        /// <returns></returns>
        public static IServiceCollection AddAutoPoco(this IServiceCollection services, Action<AutoPocoServiceOptions> options)
        {
            Check.NotNull(options, nameof(options));

            AutoPocoServiceOptions events = new AutoPocoServiceOptions();
            options(events);
            services.AddSingleton(events);

            return services.AddAutoPoco();
        }

        /// <summary>
        /// Add database specific services
        /// </summary>
        /// <param name="services">Services collection to add to</param>
        /// <returns>Services collection to chain additonal methods.</returns>
        public static IServiceCollection AddDatabaseOperations(this IServiceCollection services)
        {
            //Operations
            services.TryAddTransient<ITableOperations, TableOperations>();
            services.TryAddTransient<IViewOperations, ViewOperations>();
            services.TryAddTransient<IStoredProcedureOperations, StoredProcedureOperations>();
            services.TryAddTransient<ISchemaOperations, SchemaOperations>();

            //Resources
            services.TryAddTransient<IResourceFactory, ResourceFactory>();
            services.TryAddTransient<IAppAdminService, AppAdminService>();
            services.TryAddTransient<IRequestQueryStringService, RequestQueryStringService>();

            //logging
            services.TryAddSingleton<ITimeProvider, DefaultTimeProvider>();
            services.TryAddScoped<ILoggingService, LoggingService>();

            //db access
            services.TryAddTransient<IConnectionStringFactory, ConnectionStringFactory>();
            services.TryAddTransient<IAppDatabaseSetupService, AppDatabaseSetupService>();
            services.TryAddTransient<LogDbContext>();
            services.TryAddTransient<AppDbContext>();
            services.TryAddTransient<LoggingMigrationContext>();
            services.TryAddTransient<AppMigrationContext>();

            return services;
        }

        /// <summary>
        /// Used for provider specific configuration.
        /// </summary>
        /// <param name="services">Services collection to add to</param>
        /// <param name="options">Options to use on all registered dbcontexts.</param>
        /// <returns>Services collection to chain additonal methods.</returns>
        public static IServiceCollection ConfigureApplicationDatabase(this IServiceCollection services, Action<DbContextOptionsBuilder> options)
        {
            var dbContexts = services.Where(c => typeof(DbContext).IsAssignableFrom(c.ServiceType.BaseType))
                                     .ToList();
            foreach (var dbService in dbContexts)
            {

                typeof(ServiceCollectionExtensions).GetMethod(nameof(AddOptions), BindingFlags.NonPublic | BindingFlags.Static)
                    .MakeGenericMethod(dbService.ServiceType)
                    .Invoke(null, new object[] { services, options });
            }

            return services;
        }

        private static IServiceCollection AddOptions<TContext>(IServiceCollection services, Action<DbContextOptionsBuilder> options)
            where TContext : DbContext
        {
            void optionAction(IServiceProvider p, DbContextOptionsBuilder b) => options.Invoke(b);

            services.TryAddSingleton(c => DbContextOptionsFactory<TContext>(c, optionAction));

            return services;
        }

        private static DbContextOptions<TContext> DbContextOptionsFactory<TContext>(
             IServiceProvider applicationServiceProvider,
             Action<IServiceProvider, DbContextOptionsBuilder> optionsAction)
            where TContext : DbContext
        {
            var builder = new DbContextOptionsBuilder<TContext>(
                new DbContextOptions<TContext>(new Dictionary<Type, IDbContextOptionsExtension>()));

            builder.UseApplicationServiceProvider(applicationServiceProvider);

            optionsAction.Invoke(applicationServiceProvider, builder);

            if (typeof(TContext) == typeof(AppMigrationContext) || typeof(TContext) == typeof(LoggingMigrationContext))
                builder.ReplaceService<IMigrationsAssembly, DynamicSchema.Services.Migrations.MigrationsAssembly>();

            return builder.Options;
        }

    }
}
