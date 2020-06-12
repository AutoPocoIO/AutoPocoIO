using Microsoft.Extensions.DependencyInjection;
using System;

namespace AutoPoco.DependencyInjection
{
    internal interface IRegistratedService
    {
        Guid Id { get; }
        ServiceLifetime Lifetime { get; }
        bool IsShared { get; }
        Type ServiceType { get; }

        object Activate(IContainer container);
    }
}
