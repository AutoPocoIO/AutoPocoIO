using Microsoft.Extensions.DependencyInjection;
using System;

namespace AutoPoco.DependencyInjection
{
    internal class ServiceScope : IServiceScope
    {
        private readonly IContainer _container;

        public ServiceScope(IContainer container)
        {
            _container = container;
        }

        public IServiceProvider ServiceProvider => _container;

        public void Dispose()
        {
            _container.Dispose();
        }
    }
}
