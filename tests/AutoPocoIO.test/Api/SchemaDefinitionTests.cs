using AutoPocoIO.Api;
using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.Models;
using AutoPocoIO.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Linq;

namespace AutoPocoIO.test.Api
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class SchemaDefinitionTests : ApiOperationBase
    {
        private SchemaOperations schemaOperations;

        [TestInitialize]
        public void InitOperation()
        {
            schemaOperations = new SchemaOperations(serviceProvider);

        }

        [TestMethod]
        public void DefinitionNoLogging()
        {
            var definition = new SchemaDefinition { Name = "returned" };

            var resource = new Mock<IOperationResource>();
            resource.Setup(c => c.GetSchemaDefinition())
                .Returns(definition);

            resourceFactoryMock.Setup(c => c.GetResource("conn1", OperationType.Any, ""))
                .Returns(resource.Object);


            var result = schemaOperations.Definition("conn1");
            Assert.AreEqual(0, loggingService.LogCount);
            Assert.AreEqual(definition, result);
        }

        [TestMethod]
        public void DefinitionWithLogging()
        {
            var definition = new SchemaDefinition { Name = "returned" };

            var resource = new Mock<IOperationResource>();
            resource.Setup(c => c.GetSchemaDefinition())
                .Returns(definition);

            resourceFactoryMock.Setup(c => c.GetResource("conn1", OperationType.Any, ""))
                .Returns(resource.Object);

            var result = schemaOperations.Definition("conn1", loggingService);
            Assert.AreEqual(1, loggingService.LogCount);
            Assert.AreEqual("GET", loggingService.ApiRequests.First().RequestType);
            Assert.AreEqual(definition, result);
        }
    }
}
