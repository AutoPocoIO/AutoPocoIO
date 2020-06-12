using AutoPoco.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AutoPocoIO.AspNet.test.DependencyInjection
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class ServiceScopeTests
    {
        [TestMethod]
        public void ServiceProviderIsContainer()
        {
            var container = Mock.Of<IContainer>();
            var serviceScope = new ServiceScope(container);

            Assert.AreEqual(container, serviceScope.ServiceProvider);
        }

        [TestMethod]
        public void DisposeScopeDisposesContainer()
        {
            var container = new Mock<IContainer>();
            container.Setup(c => c.Dispose()).Verifiable();

            var serviceScope = new ServiceScope(container.Object);
            serviceScope.Dispose();

            container.Verify();

        }
    }
}
