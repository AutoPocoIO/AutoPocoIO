using AutoPocoIO.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;

namespace AutoPocoIO.Resources
{
    public class ServiceProviderCache
    {
        private readonly ConcurrentDictionary<string, IServiceProvider> _configurations
            = new ConcurrentDictionary<string, IServiceProvider>();

        public static ServiceProviderCache Instance { get; } = new ServiceProviderCache();

        public virtual IServiceProvider GetOrAdd(IOperationResource resource, IServiceProvider rootProvider)
        {
            Check.NotNull(resource, nameof(resource));

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
