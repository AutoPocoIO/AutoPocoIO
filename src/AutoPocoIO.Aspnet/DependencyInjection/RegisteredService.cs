using Microsoft.Extensions.DependencyInjection;
using System;

namespace AutoPoco.DependencyInjection
{
    internal class RegisteredService : IRegistratedService
    {
        private readonly ServiceDescriptor _descriptor;

        public RegisteredService(ServiceDescriptor descriptor)
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
            if (_descriptor.ImplementationInstance != null)
                return _descriptor.ImplementationInstance;

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
