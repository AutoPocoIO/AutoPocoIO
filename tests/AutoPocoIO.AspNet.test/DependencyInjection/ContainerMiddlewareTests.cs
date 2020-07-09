using AutoPoco.DependencyInjection;
using AutoPocoIO.AspNet.test.Owin;
using Microsoft.Owin.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Owin;
using Moq;
using System.Net.Http;
using System.Web.Http.Dependencies;
using System.Web;
using Microsoft.Owin;

namespace AutoPocoIO.AspNet.test.DependencyInjection
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class ContainerMiddlewareTests
    {

        private static IContainer containerInOwinContext;

        public class CheckContainerMiddleware : OwinMiddleware
        {
            public CheckContainerMiddleware(OwinMiddleware next) : base(next)
            {
            }

            public override Task Invoke(IOwinContext context)
            {
                containerInOwinContext = context.Get<IContainer>("autopoco:container");
                return Next.Invoke(context);
            }
        }



        [TestMethod]
        public void SetContainerIfUsingAutoPocoIOC()
        {
            var config = new HttpConfiguration();
            var container = Mock.Of<IContainer>();

            var resovler = new Mock<AutoPocoDependencyResolver>(container);
            resovler.Setup(c => c.RequestContainer).Returns(container);
            config.DependencyResolver = resovler.Object;


            using (var server = TestServer.Create(app =>
            {
                app.Use<ContainerMiddleware>(config);
                app.Use<CheckContainerMiddleware>();
            }))
            {
                var response = server.HttpClient.GetAsync("/").Result;
                Assert.AreEqual(container, containerInOwinContext);
            }
        }

        [TestMethod]
        public void IgnoreContainerIfNotUsingAutoPocoIOC()
        {
            HttpConfiguration config = new HttpConfiguration
            {
                DependencyResolver = Mock.Of<IDependencyResolver>()
            };

            using (var server = TestServer.Create(app =>
            {
                app.Use<ContainerMiddleware>(config);
                app.Use<CheckContainerMiddleware>();
            }))
            {
                var response = server.HttpClient.GetAsync("/").Result;
                Assert.IsNull(containerInOwinContext);
            }
        }

        [TestMethod]
        public void ContainerMiddlewareContinues()
        {
            var config = new HttpConfiguration();
            var container = Mock.Of<IContainer>();

            var resovler = new Mock<AutoPocoDependencyResolver>(container);
            resovler.Setup(c => c.RequestContainer).Returns(container);
            config.DependencyResolver = resovler.Object;

            using (var server = TestServer.Create(app =>
            {
                app.Use<ContainerMiddleware>(config);
                app.Use<EndOfPipeLineTestMiddleware>();
            }))
            {
                var response = server.HttpClient.GetAsync("/").Result;

                Assert.AreEqual(200, (int)response.StatusCode);
                Assert.AreEqual("end of pipeline", response.Content.ReadAsStringAsync().Result);
            }
        }
    }
}
