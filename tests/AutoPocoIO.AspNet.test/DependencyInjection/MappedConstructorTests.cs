using AutoPoco.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Linq;

namespace AutoPocoIO.AspNet.test.DependencyInjection
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class MappedConstructorTests
    {
        [TestMethod]
        public void GetEmptyConstructor()
        {
            var constructor = typeof(Class1).GetConstructors().First();
            MappedConstructor mapped = MappedConstructor.Map(constructor, Mock.Of<IContainer>());

            Assert.IsTrue(mapped.IsValid);
            Assert.IsInstanceOfType(mapped.Activate(), typeof(Class1));
        }

        [TestMethod]
        public void InjectSingleParameterFound()
        {
            IRegistratedService service;

            //new ClassWithConstructor(Class1 c1)
            var constructor = typeof(ClassWtihConstructors).GetConstructors().First(c => c.GetParameters().Length == 1);
            var container = new Mock<IContainer>();
            container.Setup(c => c.TryGetRegistration(typeof(Class1), out service)).Returns(true);

            MappedConstructor mapped = MappedConstructor.Map(constructor, container.Object);

            Assert.IsTrue(mapped.IsValid);
            Assert.IsInstanceOfType(mapped.Activate(), typeof(ClassWtihConstructors));
        }

        [TestMethod]
        public void InjectSingleParameterNotFound()
        {
            //new ClassWithConstructor(Class1 c1)
            var constructor = typeof(ClassWtihConstructors).GetConstructors().First(c => c.GetParameters().Length == 1);
            MappedConstructor mapped = MappedConstructor.Map(constructor, Mock.Of<IContainer>());

            Assert.IsFalse(mapped.IsValid);
        }

        [TestMethod]
        public void InjectMultipleParameterFound()
        {
            IRegistratedService service;

            //new ClassWithConstructor(Class1 c1, Class2 c2)
            var constructor = typeof(ClassWtihConstructors).GetConstructors().First(c => c.GetParameters().Length == 2);
            var container = new Mock<IContainer>();
            container.Setup(c => c.TryGetRegistration(typeof(Class1), out service)).Returns(true);
            container.Setup(c => c.TryGetRegistration(typeof(Class2), out service)).Returns(true);

            MappedConstructor mapped = MappedConstructor.Map(constructor, container.Object);

            Assert.IsTrue(mapped.IsValid);
            Assert.IsInstanceOfType(mapped.Activate(), typeof(ClassWtihConstructors));
        }

        [TestMethod]
        public void InjectMultipleParameterNotAllFound()
        {
            IRegistratedService service;
            //new ClassWithConstructor(Class1 c1, Class2 c2)
            var constructor = typeof(ClassWtihConstructors).GetConstructors().First(c => c.GetParameters().Length == 2);

            var container = new Mock<IContainer>();
            container.Setup(c => c.TryGetRegistration(typeof(Class1), out service)).Returns(true);

            MappedConstructor mapped = MappedConstructor.Map(constructor, container.Object);

            Assert.IsFalse(mapped.IsValid);
        }
    }
}
