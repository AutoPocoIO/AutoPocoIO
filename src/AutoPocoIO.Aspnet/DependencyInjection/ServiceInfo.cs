using System.Collections.Generic;

namespace AutoPoco.DependencyInjection
{
    internal class ServiceInfo
    {
        private IRegistratedService _lastRegistered;

        public ServiceInfo()
        {
            Implementations = new List<IRegistratedService>();
        }

        public List<IRegistratedService> Implementations { get; }

        public void AddImplementation(IRegistratedService registration)
        {
            Implementations.Add(registration);
            _lastRegistered = registration;
        }

        public bool TryGetImplementation(out IRegistratedService registration)
        {
            registration = _lastRegistered;

            return registration != null;
        }
    }
}
