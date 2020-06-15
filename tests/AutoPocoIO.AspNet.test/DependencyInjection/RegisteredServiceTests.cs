using AutoPoco.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace AutoPocoIO.AspNet.test.DependencyInjection
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class RegisteredServiceTests
    {
        [TestMethod]
        public void SetsNewGuidOnNew()
        {
            var service = new ServiceDescriptor(typeof(IInterface1), typeof(Class1), ServiceLifetime.Singleton);
            var registation = new RegisteredService(service);
            Assert.AreNotEqual(Guid.Empty, registation.Id);
        }

        [TestMethod]
        public void GetLifetimeAndServiceTypeFromDescriptor()
        {
            var service = new ServiceDescriptor(typeof(IInterface1), typeof(Class1), ServiceLifetime.Singleton);
            var registation = new RegisteredService(service);

            Assert.AreEqual(ServiceLifetime.Singleton, registation.Lifetime);
            Assert.AreEqual(typeof(IInterface1), registation.ServiceType);
        }

        [TestMethod]
        public void ScopeServiceIsShared()
        {
            var service = new ServiceDescriptor(typeof(IInterface1), typeof(Class1), ServiceLifetime.Scoped);
            var registation = new RegisteredService(service);

            Assert.IsTrue(registation.IsShared);
        }

        [TestMethod]
        public void SingletonServiceIsShared()
        {
            var service = new ServiceDescriptor(typeof(IInterface1), typeof(Class1), ServiceLifetime.Singleton);
            var registation = new RegisteredService(service);

            Assert.IsTrue(registation.IsShared);
        }

        [TestMethod]
        public void TransientServiceIsNotShared()
        {
            var service = new ServiceDescriptor(typeof(IInterface1), typeof(Class1), ServiceLifetime.Transient);
            var registation = new RegisteredService(service);

            Assert.IsFalse(registation.IsShared);
        }

        [TestMethod]
        public void ActivateFromFactory()
        {
            var container = Mock.Of<IContainer>();

            var service = new ServiceDescriptor(typeof(IInterface1), c => new Class1(), ServiceLifetime.Transient);
            var registation = new RegisteredService(service);

            var result = registation.Activate(container);
            Assert.IsInstanceOfType(result, typeof(Class1));
        }

        [TestMethod]
        public void ActivateFromDescriptor()
        {
            var container = Mock.Of<IContainer>();

            var service = new ServiceDescriptor(typeof(IInterface1), typeof(Class1), ServiceLifetime.Transient);
            var registation = new RegisteredService(service);

            var result = registation.Activate(container);
            Assert.IsInstanceOfType(result, typeof(Class1));

        }
    }
}
