using Microsoft.Extensions.DependencyInjection;

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
            return new ServiceScope(_container.BeginScope());
        }
    }
}