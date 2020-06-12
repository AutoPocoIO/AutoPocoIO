using System;
using System.Collections.Generic;

namespace AutoPoco.DependencyInjection
{
    internal class ServiceInfo
    {
        private IRegistratedService _lastRegistered;

        public ServiceInfo(Type serviceType)
        {
            Implementations = new List<IRegistratedService>();
            ServiceType = serviceType;
        }

        public Type ServiceType { get; }
        public List<IRegistratedService> Implementations { get; }

        public void AddImplementation(IRegistratedService registration)
        {
            Implementations.Add(registration);
            _lastRegistered = registration;
        }

        public bool TryGetRegistration(out IRegistratedService registration)
        {
            registration = _lastRegistered;

            return registration != null;
        }
    }
}
