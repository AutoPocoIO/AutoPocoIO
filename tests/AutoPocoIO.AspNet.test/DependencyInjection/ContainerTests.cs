using AutoPoco.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoPocoIO.AspNet.test.DependencyInjection
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class ContainerTests
    {
        [TestMethod]
        public void NewRootContainerAddsRegistration()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IInterface1, Class1>();

            var container = new Container(serviceCollection);

            PrivateObject privateObject = new PrivateObject(container);
            var registry = (IServiceRegistry)privateObject.GetField("_serviceRegistry");

            Assert.AreEqual(2, registry.Registrations.Count());
            Assert.AreEqual(container.ContainerId, container.RootContainer.ContainerId);
        }

        [TestMethod]
        public void NewContainerRegistersIContainer()
        {
            var container = new Container(new ServiceCollection());
            var containerFromSelf = container.GetService<IContainer>();
            Assert.AreEqual(container.ContainerId, containerFromSelf.ContainerId);
        }

        [TestMethod]
        public void ChildContainerHasNewIdAndSameRegistrations()
        {
            var container = new Container(new ServiceCollection());
            var childContainer = container.BeginScope();

            PrivateObject privateObject = new PrivateObject(container);
            var registry = (IServiceRegistry)privateObject.GetField("_serviceRegistry");

            privateObject = new PrivateObject(childContainer);
            var registry2 = (IServiceRegistry)privateObject.GetField("_serviceRegistry");

            Assert.AreEqual(registry, registry2);
            Assert.AreNotEqual(container.ContainerId, childContainer.ContainerId);
            Assert.AreEqual(container.ContainerId, childContainer.RootContainer.ContainerId);
        }

        [TestMethod]
        public void GetSharedInstanceOnThisScope()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddScoped<IInterface1, Class1>();

            var container = new Container(serviceCollection);
            var instance1 = (Class1)container.GetService(typeof(IInterface1));
            instance1.prop1 = 123;

            var instance2 = (Class1)container.GetService(typeof(IInterface1));
            //Keep value because its a shared instance
            Assert.AreEqual(123, instance2.prop1);
        }

        [TestMethod]
        public void GetNewInstanceInstanceOnThisScope()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient<IInterface1, Class1>();

            var container = new Container(serviceCollection);
            var instance1 = (Class1)container.GetService(typeof(IInterface1));
            instance1.prop1 = 123;

            var instance2 = (Class1)container.GetService(typeof(IInterface1));
            //Reset to default of int because its a new instance
            Assert.AreEqual(0, instance2.prop1);
        }

        [TestMethod]
        public void SharedInstanceIsNewOnChildScope()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddScoped<IInterface1, Class1>();

            var container = new Container(serviceCollection);
            var childContainer = container.BeginScope();
            var instance1 = (Class1)container.GetService(typeof(IInterface1));
            instance1.prop1 = 123;

            var instance2 = (Class1)childContainer.GetService(typeof(IInterface1));
            //Reset to default of int because its a new instance
            Assert.AreEqual(0, instance2.prop1);
        }

        [TestMethod]
        public void SharedInstanceIsSharedIfSingleton()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IInterface1, Class1>();

            var container = new Container(serviceCollection);
            var childContainer = container.BeginScope();
            var instance1 = (Class1)container.GetService(typeof(IInterface1));
            instance1.prop1 = 123;

            var instance2 = (Class1)childContainer.GetService(typeof(IInterface1));
            //Keep value because its a singleton instance
            Assert.AreEqual(123, instance2.prop1);
        }

        [TestMethod]
        public void ReturnNullIfObjectNotFound()
        {
            var container = new Container(new ServiceCollection());
            var result = container.GetService<IInterface1>();

            Assert.IsNull(result);
        }

        [TestMethod]
        public void ReturnEmptyListIfNoObjectsFoundForList()
        {
            var container = new Container(new ServiceCollection());
            var result = container.GetServices<IInterface1>();

            CollectionAssert.AreEqual(Array.Empty<IInterface1>(), result.ToList());
        }

        [TestMethod]
        public void GetServiceOfListGetsAllOfThatType()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IInterface1, Class1>();
            serviceCollection.AddSingleton<IInterface1, Class2>();

            var container = new Container(serviceCollection);
            var results = container.GetService<IEnumerable<IInterface1>>();

            Assert.AreEqual(2, results.Count());
            Assert.IsInstanceOfType(results.ElementAt(0), typeof(Class1));
            Assert.IsInstanceOfType(results.ElementAt(1), typeof(Class2));
        }

        [TestMethod]
        public void TryFindRegistrationFound()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IInterface1, Class1>();

            var container = new Container(serviceCollection);
            bool found = container.TryGetRegistration(typeof(IInterface1), out var result);

            Assert.IsTrue(found);
            Assert.AreEqual(typeof(IInterface1), result.ServiceType);
        }

        [TestMethod]
        public void TryFindRegistrationNotFound()
        {
            var container = new Container(new ServiceCollection());
            bool found = container.TryGetRegistration(typeof(IInterface1), out _);

            Assert.IsFalse(found);
        }

        [TestMethod]
        public void DisposeOfIDisposables()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient<Disposable1>();

            var container = new Container(serviceCollection);
            var instance = container.GetService<Disposable1>();
            container.Dispose();

            Assert.IsTrue(instance.IsDisposed);
        }
    }
}
