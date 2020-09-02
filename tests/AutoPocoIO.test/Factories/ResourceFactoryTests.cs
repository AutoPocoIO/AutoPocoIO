using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.Factories;
using AutoPocoIO.Models;
using AutoPocoIO.Resources;
using AutoPocoIO.Services;
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
                .Returns("type1");

            var list = new List<IOperationResource> { resource.Object };

            _resourceFactory = new ResourceFactory(appAdminService.Object, list);
        }

        [TestMethod]
        public void GetResouceByConnectorId()
        {

            appAdminService.Setup(c => c.GetConnectionById("1"))
                .Returns(new Connector() { ResourceType = "type1" });

            var resource = _resourceFactory.GetResource("1", "obj1");
            Assert.IsInstanceOfType(resource, typeof(IOperationResource));
        }

        [TestMethod]
        public void GetResouceWithOp()
        {
            appAdminService.Setup(c => c.GetConnection("conn1"))
               .Returns(new Connector() { ResourceType = "type1" });


            var resource = _resourceFactory.GetResource("conn1", OperationType.write, "obj1");
            Assert.IsInstanceOfType(resource, typeof(IOperationResource));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetNotRegisteredResource()
        {
            appAdminService.Setup(c => c.GetConnection("conn1"))
                .Returns(new Connector() { ResourceType = "type123" });

            _resourceFactory.GetResource("conn1", OperationType.read, "obj1");
        }
    }
}