using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.Factories;
using AutoPocoIO.Models;
using AutoPocoIO.Resources;
using AutoPocoIO.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace AutoPocoIO.test.Factories
{
    [Trait("Category", TestCategories.Unit)]
    public class ResourceFactoryTests
    {
        private readonly IResourceFactory _resourceFactory;
        private readonly Mock<IAppAdminService> appAdminService = new Mock<IAppAdminService>();

        public ResourceFactoryTests()
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

        [FactWithName]
        public void GetResouceByConnectorId()
        {

            appAdminService.Setup(c => c.GetConnection(1))
                .Returns(new Connector() { ResourceType = 1 });

            var resource = _resourceFactory.GetResource(1, "obj1");
            Assert.IsAssignableFrom<IOperationResource>(resource);
        }

        [FactWithName]
        public void GetResouceWithOp()
        {
            appAdminService.Setup(c => c.GetConnection("conn1"))
               .Returns(new Connector() { ResourceType = 1 });


            var resource = _resourceFactory.GetResource("conn1", OperationType.write, "obj1");
            Assert.IsAssignableFrom<IOperationResource>(resource);
        }

        [FactWithName]
        public void GetNotRegisteredResource()
        {
            appAdminService.Setup(c => c.GetConnection("conn1"))
                .Returns(new Connector() { ResourceType = 2 });


             void act() => _resourceFactory.GetResource("conn1", OperationType.read, "obj1");
            Assert.Throws<ArgumentException>(act);
        }

        [FactWithName]
        public void GetUnimplmentedResource()
        {
            appAdminService.Setup(c => c.GetConnection("conn1"))
                .Returns(new Connector() { ResourceType = 45 });

             void act() => _resourceFactory.GetResource("conn1", OperationType.read, "obj1");
            Assert.Throws<ArgumentOutOfRangeException>(act);
        }
    }
}