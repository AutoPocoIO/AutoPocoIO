using System;
using System.Collections.Generic;

namespace AutoPoco.DependencyInjection
{
    internal interface IContainer : IDisposable, IServiceProvider
    {
        Guid ContainerId { get; }
        IContainer RootContainer { get; }

        IContainer BeginScope();
        object CreateSharedInstance(Guid id, Func<object> creator);
        IEnumerable<object> GetServices(Type serviceType);
        bool TryGetRegistration(Type type, out IRegistratedService registration);
        bool TryGetSharedInstance(Guid id, out object value);
    }
}