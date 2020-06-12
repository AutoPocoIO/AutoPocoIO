using AutoPoco.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoPocoIO.AspNet.test.DependencyInjection
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class ServiceActivatorTests
    {
        [TestMethod]
        public void ActivateEmptyConstructor()
        {
            ServiceActivator activator = new ServiceActivator(typeof(Class1));
            var result = activator.Activate(Mock.Of<IContainer>());
            Assert.IsInstanceOfType(result, typeof(Class1));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void NoConstructorsAvailable()
        {
            ServiceActivator activator = new ServiceActivator(typeof(ClassWithPrivateConstructor));
            activator.Activate(Mock.Of<IContainer>());
        }

        [TestMethod]
        public void ActivateWithSingleConstructor()
        {
            IRegistratedService service;
            ServiceActivator activator = new ServiceActivator(typeof(ClassWtihSingleConstructor));
            var container = new Mock<IContainer>();
            container.Setup(c => c.TryGetRegistration(typeof(Class1), out service)).Returns(true);

            var result = activator.Activate(container.Object);
            Assert.IsInstanceOfType(result, typeof(ClassWtihSingleConstructor));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ActivateWithConstructorConflict()
        {
            IRegistratedService service;
            ServiceActivator activator = new ServiceActivator(typeof(ClassWtihTwoSingleConstructor));
            var container = new Mock<IContainer>();
            container.Setup(c => c.TryGetRegistration(typeof(Class1), out service)).Returns(true);
            container.Setup(c => c.TryGetRegistration(typeof(Class2), out service)).Returns(true);

            activator.Activate(container.Object);
        }

        [TestMethod]
        public void ActivateWithConstructorConflictButOnly1TypeRegistered()
        {
            IRegistratedService service;
            ServiceActivator activator = new ServiceActivator(typeof(ClassWtihTwoSingleConstructor));
            var container = new Mock<IContainer>();
            container.Setup(c => c.TryGetRegistration(typeof(Class1), out service)).Returns(true);

            var result = activator.Activate(container.Object);
            Assert.IsInstanceOfType(result, typeof(ClassWtihTwoSingleConstructor));
        }

        [TestMethod]
        public void ActivateWithMultipleConstructorPicksMostFound()
        {
            IRegistratedService service;
            ServiceActivator activator = new ServiceActivator(typeof(ClassWtihConstructors));
            var container = new Mock<IContainer>();
            container.Setup(c => c.TryGetRegistration(typeof(Class1), out service)).Returns(true);
            container.Setup(c => c.GetService(typeof(Class1))).Returns(new Class1());
            container.Setup(c => c.TryGetRegistration(typeof(Class2), out service)).Returns(true);
            container.Setup(c => c.GetService(typeof(Class2))).Returns(new Class2());

            ClassWtihConstructors result = (ClassWtihConstructors)activator.Activate(container.Object);
            Assert.IsNotNull(result.c1);
            Assert.IsNotNull(result.c2);
        }

        [TestMethod]
        public void ActivateWithMultipleConstructorPicksMostFoundMissingOne()
        {
            IRegistratedService service;
            ServiceActivator activator = new ServiceActivator(typeof(ClassWtihConstructors));
            var container = new Mock<IContainer>();
            container.Setup(c => c.TryGetRegistration(typeof(Class1), out service)).Returns(true);
            container.Setup(c => c.GetService(typeof(Class1))).Returns(new Class1());
            container.Setup(c => c.TryGetRegistration(typeof(Class2), out service)).Returns(false);
            container.Setup(c => c.GetService(typeof(Class2))).Returns(new Class2());

            ClassWtihConstructors result = (ClassWtihConstructors)activator.Activate(container.Object);
            Assert.IsNotNull(result.c1);
            Assert.IsNull(result.c2);
        }

        [TestMethod]
        public void ActivateWithListParameter()
        {
            IRegistratedService service;
            ServiceActivator activator = new ServiceActivator(typeof(ClassWithListParameter));
            var container = new Mock<IContainer>();
            container.Setup(c => c.TryGetRegistration(typeof(IEnumerable<IInterface1>), out service)).Returns(true);
            container.Setup(c => c.GetServices(typeof(IInterface1)))
                .Returns(new List<IInterface1> { new Class1(), new Class2() });

            ClassWithListParameter result = (ClassWithListParameter)activator.Activate(container.Object);
            Assert.AreEqual(2, result.c1.Count());
        }
    }
}
