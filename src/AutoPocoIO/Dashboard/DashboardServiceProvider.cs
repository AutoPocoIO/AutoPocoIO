using AutoPocoIO.Context;
using AutoPocoIO.Dashboard.Pages;
using AutoPocoIO.Dashboard.Repo;
using AutoPocoIO.Dashboard.Repos;
using AutoPocoIO.Factories;
using AutoPocoIO.Resources;
using AutoPocoIO.Services;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;

namespace AutoPocoIO.Dashboard
{
    internal class DashboardServiceProvider
    {
        private static IServiceProvider _provider;

        public DashboardServiceProvider() { }

        public static DashboardServiceProvider Instance { get; } = new DashboardServiceProvider();

        public virtual IServiceProvider GetServiceProvider(IServiceProvider rootProvider)
        {
            if (_provider == null)
            {
                var services = new ServiceCollection();

                services.AddSingleton<DashboardRoutes>();
                services.TryAddSingleton<ITimeProvider, DefaultTimeProvider>();


                var builders = rootProvider.GetService<IEnumerable<IConnectionStringBuilder>>();
                foreach (var builder in builders)
                    services.TryAddTransient(c => builder);

                services.AddScoped<IConnectionStringFactory, ConnectionStringFactory>();

                services.AddScoped<AppDbContext>();
                services.AddScoped<LogDbContext>();

                var appOptions = rootProvider.GetRequiredService<DbContextOptions<AppDbContext>>();
                var logOptions = rootProvider.GetRequiredService<DbContextOptions<LogDbContext>>();
                services.AddScoped(c => appOptions);
                services.AddScoped(c => logOptions);


                //Connector
                services.AddTransient<IConnectorRepo, ConnectorRepo>();
                services.AddTransient<ConnectorsPage>();
                services.AddTransient<ConnectorForm>();

                services.AddScoped<IDashboardRepo, DashboardRepo>();
                services.AddScoped<DashboardPage>();

                services.AddScoped<Layout>();
                _provider = services.BuildServiceProvider();
            }

            return _provider;
        }
    }
}
