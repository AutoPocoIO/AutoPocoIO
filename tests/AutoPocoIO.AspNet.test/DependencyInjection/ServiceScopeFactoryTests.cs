using AutoPoco.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AutoPocoIO.AspNet.test.DependencyInjection
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class ServiceScopeFactoryTests
    {
        [TestMethod]
        public void CreateScopeCreatesContainerFromRoot()
        {
            var scopedContainer = Mock.Of<IContainer>();
            var container = new Mock<IContainer>();
            container.Setup(c => c.BeginScope()).Returns(scopedContainer);

            var serviceScopeFactory = new ServiceScopeFactory(container.Object);
            var result = serviceScopeFactory.CreateScope();

            Assert.AreEqual(scopedContainer, result.ServiceProvider);
        }
    }
}
