using AutoPocoIO.Api;
using AutoPocoIO.Context;
using AutoPocoIO.EntityConfiguration;
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
    public static class ServiceExtensions
    {
        /// <summary>
        /// Adds the minimum essential AutoPoco services to the specified <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/>
        /// </summary>
        /// <param name="services"> The <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/> to add services to.</param>
        /// <param name="options">Events to fire before or after logging.</param>
        /// <returns></returns>
        public static IServiceCollection AddAutoPoco(this IServiceCollection services, Action<AutoPocoServiceOptions> options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            AutoPocoServiceOptions events = new AutoPocoServiceOptions();
            options(events);
            services.AddSingleton(events);

            return services.AddAutoPoco();
        }
    }
}
