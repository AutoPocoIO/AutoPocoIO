using AutoPoco.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;

namespace AutoPocoIO.AspNet.test.DependencyInjection
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class DependencyScopeTests
    {
        [TestMethod]
        public void GetMultipleServices()
        {
            var services = new List<IInterface1> { new Class1() };
            var container = new Mock<IContainer>();
            container.Setup(c => c.GetServices(typeof(IInterface1)))
                .Returns(services);

            var scope = new AutoPocoDependencyScope(container.Object);
            var result = scope.GetServices(typeof(IInterface1));

            Assert.AreEqual(services, result);
        }

        [TestMethod]
        public void GetSingleService()
        {
            var service = new Class1();
            var container = new Mock<IContainer>();
            container.Setup(c => c.GetService(typeof(IInterface1)))
                .Returns(service);

            var scope = new AutoPocoDependencyScope(container.Object);
            var result = scope.GetService(typeof(IInterface1));

            Assert.AreEqual(service, result);
        }

        [TestMethod]
        public void DisposeOfScopeDisposesContainer()
        {
            var container = new Mock<IContainer>();
            container.Setup(c => c.Dispose()).Verifiable();

            var scope = new AutoPocoDependencyScope(container.Object);
            scope.Dispose();

            container.Verify();
        }
    }
}
