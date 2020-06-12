using System;
using System.Collections.Generic;

namespace AutoPoco.DependencyInjection
{
    internal interface IServiceRegistry
    {
        void AddRegistration(IRegistratedService registration);
        IList<IRegistratedService> Registrations { get; }
        bool TryGetRegistration(Type service, out IRegistratedService registration);
        ServiceInfo GetServiceInfo(Type serviceType);
    }
}
