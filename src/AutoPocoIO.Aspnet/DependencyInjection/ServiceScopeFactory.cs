using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoPoco.DependencyInjection
{
    internal class ServiceScopeFactory : IServiceScopeFactory
    {
        private readonly IContainer _container;

        public ServiceScopeFactory(IContainer container)
        {
            _container = container;
        }

        public IServiceScope CreateScope()
        {
            return new ServiceScope(this._container.BeginScope());
        }
    }
}
