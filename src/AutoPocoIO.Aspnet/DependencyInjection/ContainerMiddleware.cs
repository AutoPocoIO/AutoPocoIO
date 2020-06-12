using Microsoft.Owin;
using System.Threading.Tasks;
using System.Web.Http;

namespace AutoPoco.DependencyInjection
{
    internal class ContainerMiddleware : OwinMiddleware
    {
        private readonly HttpConfiguration _config;
        public ContainerMiddleware(OwinMiddleware next, HttpConfiguration config) : base(next)
        {
            _config = config;
        }

        public override Task Invoke(IOwinContext context)
        {
            if (_config.DependencyResolver is AutoPocoDependencyResolver resolver)
                context.Set("autopoco:container", resolver.RequestContainer);

            return Next.Invoke(context);
        }
    }
}
