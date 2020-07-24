using AutoPoco.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace AutoPocoIO.AspNet.test.DependencyInjection
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class ServiceRegistryTests
    {
        ServiceRegistry registry;

        [TestInitialize]
        public void Init()
        {
            registry = new ServiceRegistry();

            var _services = InternalServiceInfo;
            _services.Clear();
        }

        private ConcurrentDictionary<Type, ServiceInfo> InternalServiceInfo
        {
            get
            {
                PrivateObject regisry = new PrivateObject(registry);
                return (ConcurrentDictionary<Type, ServiceInfo>)regisry.GetField("_serviceInfo");
            }
        }

        [TestMethod]
        public void GetServiceInfoNotRegisteredYet()
        {
            Assert.AreEqual(0, InternalServiceInfo.Count);

            registry.GetServiceInfo(typeof(Class1));

            Assert.AreEqual(1, InternalServiceInfo.Count);
        }

        [TestMethod]
        public void GetServiceInfoOnlyAddsOncePerType()
        {
            Assert.AreEqual(0, InternalServiceInfo.Count);

            var serviceInfo1 = registry.GetServiceInfo(typeof(Class1));

            Assert.AreEqual(1, InternalServiceInfo.Count);

            var serviceInfo2 = registry.GetServiceInfo(typeof(Class1));

            Assert.AreEqual(1, InternalServiceInfo.Count);
            Assert.AreEqual(serviceInfo1, serviceInfo2);
        }

        [TestMethod]
        public void AddInitialRegistrationAddsToRegistrationsAndServiceInfo()
        {
            var id = Guid.NewGuid();

            var registeredService = new Mock<IRegistratedService>();
            registeredService.Setup(c => c.ServiceType).Returns(typeof(IInterface1));
            registeredService.Setup(c => c.Id).Returns(id);

            registry.AddRegistration(registeredService.Object);

            Assert.AreEqual(registeredService.Object, InternalServiceInfo[typeof(IInterface1)].Implementations.Single());
            Assert.AreEqual(registeredService.Object, registry.Registrations.Single());
        }

        [TestMethod]
        public void AddSecondServiceAddsSecondImplmenentaion()
        {
            var id = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var registeredService = new Mock<IRegistratedService>();
            registeredService.Setup(c => c.ServiceType).Returns(typeof(IInterface1));
            registeredService.Setup(c => c.Id).Returns(id);

            var registeredService2 = new Mock<IRegistratedService>();
            registeredService2.Setup(c => c.ServiceType).Returns(typeof(IInterface1));
            registeredService2.Setup(c => c.Id).Returns(id2);


            registry.AddRegistration(registeredService.Object);
            registry.AddRegistration(registeredService2.Object);

            Assert.AreEqual(2, InternalServiceInfo[typeof(IInterface1)].Implementations.Count());
            Assert.AreEqual(2, registry.Registrations.Count());

            Assert.AreEqual(id, InternalServiceInfo[typeof(IInterface1)].Implementations.ElementAt(0).Id);
            Assert.AreEqual(id2, InternalServiceInfo[typeof(IInterface1)].Implementations.ElementAt(1).Id);

            Assert.AreEqual(id, registry.Registrations.ElementAt(0).Id);
            Assert.AreEqual(id2, registry.Registrations.ElementAt(1).Id);
        }

        [TestMethod]
        public void GetObjectRegistrationFound()
        {
            var registeredService = new Mock<IRegistratedService>();
            registeredService.Setup(c => c.ServiceType).Returns(typeof(IInterface1));

            registry.AddRegistration(registeredService.Object);

            bool found = registry.TryGetRegistration(typeof(IInterface1), out IRegistratedService foundService);

            Assert.IsTrue(found);
            Assert.AreEqual(registeredService.Object, foundService);
        }

        [TestMethod]
        public void GetObjectRegistrationNotFound()
        {
            bool found = registry.TryGetRegistration(typeof(IInterface1), out IRegistratedService foundService);

            Assert.IsFalse(found);
            Assert.IsNull(foundService);
        }

        [TestMethod]
        public void GetListRegistrationFound()
        {
            var registeredService = new Mock<IRegistratedService>();
            registeredService.Setup(c => c.ServiceType).Returns(typeof(IInterface1));

            registry.AddRegistration(registeredService.Object);

            bool found = registry.TryGetRegistration(typeof(IEnumerable<IInterface1>), out IRegistratedService foundService);

            Assert.IsTrue(found);
            Assert.AreEqual(registeredService.Object, foundService);
        }

        [TestMethod]
        public void GetListRegistrationNotFound()
        {
            bool found = registry.TryGetRegistration(typeof(IEnumerable<IInterface1>), out IRegistratedService foundService);

            Assert.IsFalse(found);
            Assert.IsNull(foundService);
        }
    }
}
