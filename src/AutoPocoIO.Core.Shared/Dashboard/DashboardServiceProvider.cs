﻿using AutoPocoIO.Context;
using AutoPocoIO.Dashboard.Pages;
using AutoPocoIO.Dashboard.Repos;
using AutoPocoIO.EntityConfiguration;
using AutoPocoIO.Factories;
using AutoPocoIO.Middleware;
using AutoPocoIO.Resources;
using AutoPocoIO.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;

namespace AutoPocoIO.Dashboard
{
    /// <summary>
    /// Internal dashboard scoped services.
    /// </summary>
    public class DashboardServiceProvider
    {
        private static IServiceProvider _provider;

        /// <summary>
        /// Initalize service provider.
        /// </summary>
        public DashboardServiceProvider() { }

        /// <summary>
        /// Get a single static instance of the service provider
        /// </summary>
        public static DashboardServiceProvider Instance { get; } = new DashboardServiceProvider();

        /// <summary>
        /// Register services if this is the first time using the middleware
        /// </summary>
        /// <param name="rootProvider">Application root provider</param>
        /// <returns></returns>
        public virtual IServiceProvider GetServiceProvider(IServiceProvider rootProvider)
        {
            if (_provider == null)
            {
                IServiceCollection services = new ServiceCollection();

                services.AddSingleton<DashboardRoutes>();
                services.TryAddSingleton<ITimeProvider, DefaultTimeProvider>();


                var builders = rootProvider.GetService<IEnumerable<IConnectionStringBuilder>>();
                foreach (var builder in builders)
                    services.TryAddTransient(typeof(IConnectionStringBuilder), builder.GetType());

                var resources = rootProvider.GetService<IEnumerable<IOperationResource>>();
                foreach (var resource in resources)
                    services.TryAddTransient(typeof(IOperationResource), resource.GetType());

                services.AddScoped<IConnectionStringFactory, ConnectionStringFactory>();
                services.AddScoped<IResourceFactory, ResourceFactory>();
                services.AddTransient<IAppAdminService, AppAdminService>();

                services.AddScoped<AppDbContext>();
                services.AddScoped<LogDbContext>();

                var appOptions = rootProvider.GetRequiredService<DbContextOptions<AppDbContext>>();
                var logOptions = rootProvider.GetRequiredService<DbContextOptions<LogDbContext>>();
                services.AddScoped(c => appOptions);
                services.AddScoped(c => logOptions);

                var contextConfig = rootProvider.GetService<IContextEntityConfiguration>();
                if (contextConfig != null)
                    services.TryAddTransient(c => contextConfig);


                //Connector
                services.AddTransient<IConnectorRepo, ConnectorRepo>();
                services.AddTransient<ConnectorsPage>();
                services.AddTransient<ConnectorForm>();

                //DataDictionary
                services.AddTransient<IDataDictionaryRepo, DataDictionaryRepo>();
                services.AddTransient<DataDictionaryPage>();
                services.AddTransient<SchemaPage>();
                services.AddTransient<ObjectDetailsPage>();

                //Request
                services.AddTransient<IRequestHistoryRepo, RequestHistoryRepo>();
                services.AddTransient<RequestHistoryPage>();


                services.AddScoped<IDashboardRepo, DashboardRepo>();
                services.AddScoped<DashboardPage>();

                services.AddTransient<ILayoutPage, Layout>();

                var replaceServices = rootProvider.GetService<IReplaceServices<DashboardServiceProvider>>();
                if (replaceServices != null)
                    services = replaceServices.ReplaceInternalServices(rootProvider, services);

                _provider = services.BuildServiceProvider();
            }

            return _provider;
        }
    }
}
