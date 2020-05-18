using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.Factories;
using AutoPocoIO.Models;
using AutoPocoIO.Resources;
using AutoPocoIO.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace AutoPocoIO.test.Factories
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class ResourceFactoryTests
    {
        private IResourceFactory _resourceFactory;
        private readonly Mock<IAppAdminService> appAdminService = new Mock<IAppAdminService>();

        [TestInitialize]
        public void Init()
        {
            var resource = new Mock<IOperationResource>();
            resource.Setup(c => c.ResourceType)
                .Returns(ResourceType.Mssql);

            var list = new List<IOperationResource> { resource.Object };

            var services = new ServiceCollection()
                .AddSingleton(appAdminService.Object)
                .BuildServiceProvider();

            _resourceFactory = new ResourceFactory(services, list);
        }

        [TestMethod]
        public void GetResouceByConnectorId()
        {

            appAdminService.Setup(c => c.GetConnection(1))
                .Returns(new Connector() { ResourceType = 1 });

            var resource = _resourceFactory.GetResource(1, "obj1");
            Assert.IsInstanceOfType(resource, typeof(IOperationResource));
        }

        [TestMethod]
        public void GetResouceWithOp()
        {
            appAdminService.Setup(c => c.GetConnection("conn1"))
               .Returns(new Connector() { ResourceType = 1 });


            var resource = _resourceFactory.GetResource("conn1", OperationType.write, "obj1");
            Assert.IsInstanceOfType(resource, typeof(IOperationResource));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetNotRegisteredResource()
        {
            appAdminService.Setup(c => c.GetConnection("conn1"))
                .Returns(new Connector() { ResourceType = 2 });

            _resourceFactory.GetResource("conn1", OperationType.read, "obj1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetUnimplmentedResource()
        {
            appAdminService.Setup(c => c.GetConnection("conn1"))
                .Returns(new Connector() { ResourceType = 45 });

            _resourceFactory.GetResource("conn1", OperationType.read, "obj1");
        }
    }
}