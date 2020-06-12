using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;

namespace AutoPoco.DependencyInjection
{
    internal class AutoPocoDependencyScope : IDependencyScope
    {
        private readonly IContainer _container;

        public AutoPocoDependencyScope(IContainer container)
        {
            _container = container;
        }

        public void Dispose()
        {
            _container.Dispose();
        }

        public object GetService(Type serviceType)
        {
            return _container.GetService(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return _container.GetServices(serviceType);
        }
    }
}
