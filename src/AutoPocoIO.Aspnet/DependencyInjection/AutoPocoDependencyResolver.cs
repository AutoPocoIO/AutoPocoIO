using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Http.Dependencies;

namespace AutoPoco.DependencyInjection
{
    internal class AutoPocoDependencyResolver : System.Web.Mvc.IDependencyResolver, IDependencyResolver
    {
        private readonly IContainer _container;

        public AutoPocoDependencyResolver(IContainer container)
        {
            this._container = container;
        }
        public IDependencyScope BeginScope()
        {
            return new AutoPocoDependencyScope(_container.BeginScope());
        }

        public void Dispose()
        {
            RequestContainer.Dispose();
        }

        public object GetService(Type serviceType)
        {
            return RequestContainer.GetService(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return RequestContainer.GetServices(serviceType);
        }


        public virtual IContainer RequestContainer
        {
            get
            {
                var requestContainer = (IContainer)HttpContext.Current.Items[typeof(IContainer)];
                if (requestContainer == null)
                {
                    requestContainer = _container.BeginScope();
                    RequestScopeManagement.ScopedContainer = requestContainer;
                    HttpContext.Current.Items[typeof(IContainer)] = requestContainer;
                }


                return requestContainer;
            }
        }
    }
}
