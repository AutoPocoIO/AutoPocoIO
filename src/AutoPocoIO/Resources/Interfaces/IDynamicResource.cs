using AutoPocoIO.DynamicSchema.Db;
using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.Models;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AutoPocoIO.Resources
{
    public interface IDynamicResource
    {
        IDbSchema DbSchema { get; }

        Connector Connector { get; }
        string DatabaseName { get; }
        string DbObjectName { get; }
        string SchemaName { get; }
        ResourceType ResourceType { get; }

        void ApplyServices(IServiceCollection serviceCollection, IServiceProvider rootProvider);

    }
}
