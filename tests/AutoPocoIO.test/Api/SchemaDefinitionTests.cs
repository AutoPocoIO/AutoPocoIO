using AutoPocoIO.Api;
using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.Models;
using AutoPocoIO.Resources;
using Xunit;
using Moq;
using System.Linq;

namespace AutoPocoIO.test.Api
{
    
     [Trait("Category", TestCategories.Unit)]
    public class SchemaDefinitionTests : ApiOperationBase
    {
        private readonly SchemaOperations schemaOperations;

        public SchemaDefinitionTests()
        {
            schemaOperations = new SchemaOperations(serviceProvider);

        }

        [FactWithName]
        public void DefinitionNoLogging()
        {
            var definition = new SchemaDefinition { Name = "returned" };

            var resource = new Mock<IOperationResource>();
            resource.Setup(c => c.GetSchemaDefinition())
                .Returns(definition);

            resourceFactoryMock.Setup(c => c.GetResource("conn1", OperationType.Any, ""))
                .Returns(resource.Object);


            var result = schemaOperations.Definition("conn1");
            Assert.Equal(0, loggingService.LogCount);
            Assert.Equal(definition, result);
        }

        [FactWithName]
        public void DefinitionWithLogging()
        {
            var definition = new SchemaDefinition { Name = "returned" };

            var resource = new Mock<IOperationResource>();
            resource.Setup(c => c.GetSchemaDefinition())
                .Returns(definition);

            resourceFactoryMock.Setup(c => c.GetResource("conn1", OperationType.Any, ""))
                .Returns(resource.Object);

            var result = schemaOperations.Definition("conn1", loggingService);
            Assert.Equal(1, loggingService.LogCount);
            Assert.Equal("GET", loggingService.ApiRequests.First().RequestType);
            Assert.Equal(definition, result);
        }
    }
}
