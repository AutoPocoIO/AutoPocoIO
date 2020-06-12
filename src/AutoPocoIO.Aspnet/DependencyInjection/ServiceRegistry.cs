using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AutoPoco.DependencyInjection
{
    internal class ServiceRegistry : IServiceRegistry
    {
        private readonly ConcurrentDictionary<Type, ServiceInfo> _serviceInfo = new ConcurrentDictionary<Type, ServiceInfo>();

        public ServiceRegistry()
        {
            Registrations = new List<IRegistratedService>();
        }

        public IList<IRegistratedService> Registrations { get; }

        public void AddRegistration(IRegistratedService registration)
        {
            ServiceInfo service = GetServiceInfo(registration.ServiceType);
            service.AddImplementation(registration);
            Registrations.Add(registration);
        }

        public ServiceInfo GetServiceInfo(Type serviceType)
        {
            if (!_serviceInfo.TryGetValue(serviceType, out ServiceInfo service))
            {
                service = new ServiceInfo();
                _serviceInfo.TryAdd(serviceType, service);
            }

            return service;
        }

        public bool TryGetRegistration(Type service, out IRegistratedService registration)
        {
            ServiceInfo serviceInfo;
            if (typeof(IEnumerable).IsAssignableFrom(service))
                serviceInfo = GetServiceInfo(service.GenericTypeArguments[0]);
            else
                serviceInfo = GetServiceInfo(service);
            return serviceInfo.TryGetImplementation(out registration);
        }
    }
}