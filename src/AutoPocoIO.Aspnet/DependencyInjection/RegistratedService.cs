using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoPoco.DependencyInjection
{
    internal class RegistratedService : IRegistratedService
    {
        private ServiceDescriptor _descriptor;

        public RegistratedService(ServiceDescriptor descriptor)
        {
            Id = Guid.NewGuid();
            _descriptor = descriptor;
        }

        public Guid Id { get; }

        public ServiceLifetime Lifetime => _descriptor.Lifetime;
        public Type ServiceType => _descriptor.ServiceType;

        public bool IsShared
        {
            get
            {
                switch (Lifetime)
                {
                    case ServiceLifetime.Scoped:
                    case ServiceLifetime.Singleton:
                        return true;
                    default:
                        return false;
                }
            }
        }


        public object Activate(IContainer container)
        {
            if (_descriptor.ImplementationFactory == null)
            {
                var activator = new ServiceActivator(_descriptor.ImplementationType);
                return activator.Activate(container);
            }
            else
            {
                return _descriptor.ImplementationFactory.Invoke(container);
            }
        }
    }
}
