using AutoPoco.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AutoPocoIO.AspNet.test.DependencyInjection
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class ServiceInfoTests
    {
        [TestMethod]
        public void AddImplmenetationAddsToList()
        {
            ServiceInfo service = new ServiceInfo();

            var registration = new Mock<IRegistratedService>();
            registration.Setup(c => c.ServiceType).Returns(typeof(IInterface1));

            service.AddImplementation(registration.Object);

            CollectionAssert.AreEqual(new[] { registration.Object }, service.Implementations);
        }

        [TestMethod]
        public void GetImplementationFound()
        {
            ServiceInfo service = new ServiceInfo();

            var registration = new Mock<IRegistratedService>();
            registration.Setup(c => c.ServiceType).Returns(typeof(IInterface1));

            service.AddImplementation(registration.Object);

            bool found = service.TryGetImplementation(out IRegistratedService foundService);

            Assert.IsTrue(found);
            Assert.AreEqual(registration.Object, foundService);
        }

        [TestMethod]
        public void GetImplementationNotFound()
        {
            ServiceInfo service = new ServiceInfo();

            var registration = new Mock<IRegistratedService>();
            registration.Setup(c => c.ServiceType).Returns(typeof(IInterface1));

            bool found = service.TryGetImplementation(out IRegistratedService foundService);

            Assert.IsFalse(found);
            Assert.IsNull(foundService);
        }

        [TestMethod]
        public void GetLastRegisteredServiceIfMultiple()
        {
            ServiceInfo service = new ServiceInfo();

            var registration = new Mock<IRegistratedService>();
            registration.Setup(c => c.ServiceType).Returns(typeof(IInterface1));

            var registration2 = new Mock<IRegistratedService>();
            registration2.Setup(c => c.ServiceType).Returns(typeof(IInterface1));

            service.AddImplementation(registration.Object);
            service.AddImplementation(registration2.Object);

            bool found = service.TryGetImplementation(out IRegistratedService foundService);

            Assert.IsTrue(found);
            Assert.AreEqual(registration2.Object, foundService);
        }
    }
}
