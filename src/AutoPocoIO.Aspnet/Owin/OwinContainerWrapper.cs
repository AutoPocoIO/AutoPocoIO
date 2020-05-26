using AutoPocoIO.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace AutoPocoIO.Owin
{
    internal class OwinContainerWrapper<T> : OwinMiddleware
        where T : class, IOwinMiddlewareWithDI
    {
        private readonly HttpConfiguration _config;

        public OwinContainerWrapper(OwinMiddleware next, HttpConfiguration config)
            : base(next)
        {
            _config = config;
        }

        public override Task Invoke(IOwinContext context)
        {
            T middleware = (T)null;
            //Attempt to resolve autofacscope
            var key = context.Environment.Keys.FirstOrDefault(c => c.StartsWith("autofac:OwinLifetimeScope:", StringComparison.OrdinalIgnoreCase));
            if (key != null)
            {
                var scope = (IServiceProvider)context.Environment[key];
                middleware = scope.GetRequiredService<T>();
            }

            //Fall back
            if (middleware == null)
                middleware = _config.DependencyResolver.GetRequiredService<T>();

            middleware.NextComponent = Next;
            return middleware.Invoke(context);
        }
    }
}
