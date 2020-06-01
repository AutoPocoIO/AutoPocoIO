using AutoPocoIO.Extensions;
using AutoPocoIO.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Testing;
using Xunit;
using Moq;
using Owin;
using System;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dependencies;

namespace AutoPocoIO.AspNet.test.Owin
{
    
    [Trait("Category", TestCategories.Unit)]
    public class OwinContainerWrapperTests
    {
        private class DiMiddleware : IOwinMiddlewareWithDI
        {
            public OwinMiddleware NextComponent { get; set; }

            public Task Invoke(IOwinContext context)
            {
                return NextComponent.Invoke(context);
            }
        }

        private class SetAutoFacEnvironment : OwinMiddleware
        {
            private readonly IServiceProvider _provider;
            public SetAutoFacEnvironment(OwinMiddleware next, IServiceProvider provider) : base(next)
            {
                _provider = provider;
            }

            public override Task Invoke(IOwinContext context)
            {
                context.Environment[$"autofac:OwinLifetimeScope:{Guid.NewGuid()}"] = _provider;
                return Next.Invoke(context);
            }
        }


        [FactWithName]
        public void ResolveMiddlewareFromConfig()
        {
            var provider = new Mock<IDependencyResolver>();
            provider.Setup(c => c.GetService(typeof(DiMiddleware))).Returns(new DiMiddleware());

            HttpConfiguration config = new HttpConfiguration
            {
                DependencyResolver = provider.Object
            };

            using (var server = TestServer.Create(app =>
            {
                app.UseWithDependencyInjection<DiMiddleware>(config);
                app.Use<EndOfPipeLineTestMiddleware>();
            }))
            {
                var response = server.HttpClient.GetAsync("/").Result;
                Assert.Equal(200, (int)response.StatusCode);
                Assert.Equal("end of pipeline", response.Content.ReadAsStringAsync().Result);
            }
        }

        [FactWithName]
        public void ResolveMiddlewareFromAutoFac()
        {
            var provider = new Mock<IServiceProvider>();
            provider.Setup(c => c.GetService(typeof(DiMiddleware))).Returns(new DiMiddleware());

            using (var server = TestServer.Create(app =>
            {
                app.Use<SetAutoFacEnvironment>(provider.Object);
                app.UseWithDependencyInjection<DiMiddleware>(null);
                app.Use<EndOfPipeLineTestMiddleware>();
            }))
            {
                var response = server.HttpClient.GetAsync("/").Result;
                Assert.Equal(200, (int)response.StatusCode);
                Assert.Equal("end of pipeline", response.Content.ReadAsStringAsync().Result);
            }
        }
    }
}
