using AutoPoco.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace AutoPocoIO.AspNet.test.DependencyInjection
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class DependencyResolverTests
    {
        [TestMethod]
        public void BeginScopeCreatesNewContainerScope()
        {
            var rootContainer = new Mock<IContainer>();
            var scopedContainer = new Mock<IContainer>();
            rootContainer.Setup(c => c.BeginScope()).Returns(scopedContainer.Object);
            scopedContainer.Setup(c => c.GetService(typeof(IInterface1))).Returns(new Class1());

            var depResolver = new AutoPocoDependencyResolver(rootContainer.Object);
            var scope = depResolver.BeginScope();

            var result = scope.GetService(typeof(IInterface1));

            Assert.IsInstanceOfType(result, typeof(Class1));

        }

        [TestMethod]
        public void GetRequestContainerFromContext()
        {
            var request = new HttpRequest("", "http://test.com", "");
            var response = new HttpResponse(new StreamWriter(new MemoryStream()));
            var context = new HttpContext(request, response);

            var container = Mock.Of<IContainer>();
            context.Items[typeof(IContainer)] = container;

            HttpContext.Current = context;
            var depResolver = new AutoPocoDependencyResolver(Mock.Of<IContainer>());

            Assert.AreEqual(container, depResolver.RequestContainer);
        }

        [TestMethod]
        public void BeginRequestScopeAndSetToContext()
        {
            var request = new HttpRequest("", "http://test.com", "");
            var response = new HttpResponse(new StreamWriter(new MemoryStream()));
            var context = new HttpContext(request, response);

            var rootContainer = new Mock<IContainer>();
            var container = Mock.Of<IContainer>();
            rootContainer.Setup(c => c.BeginScope()).Returns(container);

            HttpContext.Current = context;
            var depResolver = new AutoPocoDependencyResolver(rootContainer.Object);

            Assert.AreEqual(container, depResolver.RequestContainer);
            Assert.AreEqual(container, context.Items[typeof(IContainer)]);
            Assert.AreEqual(container, RequestScopeManagement.ScopedContainer);
        }

        [TestMethod]
        public void GetServiceFromRequestContext()
        {
            var request = new HttpRequest("", "http://test.com", "");
            var response = new HttpResponse(new StreamWriter(new MemoryStream()));
            var context = new HttpContext(request, response);

            var container = new Mock<IContainer>();
            container.Setup(c => c.GetService(typeof(IInterface1))).Returns(new Class1());
            context.Items[typeof(IContainer)] = container.Object;

            HttpContext.Current = context;
            var depResolver = new AutoPocoDependencyResolver(Mock.Of<IContainer>());
            var result = depResolver.GetService(typeof(IInterface1));

            Assert.IsInstanceOfType(result, typeof(Class1));
        }

        [TestMethod]
        public void GetServicesFromRequestContext()
        {
            var request = new HttpRequest("", "http://test.com", "");
            var response = new HttpResponse(new StreamWriter(new MemoryStream()));
            var context = new HttpContext(request, response);

            var container = new Mock<IContainer>();
            var class1 = new Class1();
            container.Setup(c => c.GetServices(typeof(IInterface1))).Returns(new List<IInterface1> { class1 });
            context.Items[typeof(IContainer)] = container.Object;

            HttpContext.Current = context;
            var depResolver = new AutoPocoDependencyResolver(Mock.Of<IContainer>());
            var result = depResolver.GetServices(typeof(IInterface1));

            CollectionAssert.AreEqual(new[] { class1 }, result.ToList());
        }

        [TestMethod]
        public void DisposeFromRequestContext()
        {
            var request = new HttpRequest("", "http://test.com", "");
            var response = new HttpResponse(new StreamWriter(new MemoryStream()));
            var context = new HttpContext(request, response);

            var container = new Mock<IContainer>();
            var class1 = new Class1();
            container.Setup(c => c.Dispose()).Verifiable();

            context.Items[typeof(IContainer)] = container.Object;

            HttpContext.Current = context;
            var depResolver = new AutoPocoDependencyResolver(Mock.Of<IContainer>());
            depResolver.Dispose();

            container.Verify();
        }
    }
}
