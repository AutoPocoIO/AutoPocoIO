using AutoPocoIO.DynamicSchema.Enums;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;

namespace AutoPocoIO.Resources
{
    internal class ServiceProviderCache
    {
        private readonly ConcurrentDictionary<ResourceType, IServiceProvider> _configurations
            = new ConcurrentDictionary<ResourceType, IServiceProvider>();

        public static ServiceProviderCache Instance { get; } = new ServiceProviderCache();

        public virtual IServiceProvider GetOrAdd(IOperationResource resource, IServiceProvider rootProvider)
        {
            return _configurations.GetOrAdd(
               resource.ResourceType,
               k =>
               {
                   var service = new ServiceCollection();

                   resource.ApplyServices(service, rootProvider);

                   return service.BuildServiceProvider();

               });
        }
    }
}
