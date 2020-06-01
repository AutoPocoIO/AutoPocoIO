using AutoPocoIO.Api;
using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.Models;
using AutoPocoIO.Resources;
using Xunit;
using Moq;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace AutoPocoIO.test.Api
{
    
     [Trait("Category", TestCategories.Unit)]
    public class StoredProcOperationsNoLoggingTests : ApiOperationBase
    {
        private readonly StoredProcedureOperations storedProcedureOperations;
        public StoredProcOperationsNoLoggingTests()
        {
            storedProcedureOperations = new StoredProcedureOperations(serviceProvider);
        }

        [FactWithName]
        public void ExecuteNoParams()
        {
            var obj = new IQueryableType2();

            var resource = new Mock<IOperationResource>();
            resource.Setup(c => c.ExecuteProc(new Dictionary<string, object>()))
               .Returns(obj);

            resourceFactoryMock.Setup(c => c.GetResource("conn1",OperationType.read, "proc1"))
                .Returns(resource.Object);

            var results = storedProcedureOperations.ExecuteNoParameters("conn1", "proc1");
            Assert.Equal(0, loggingService.LogCount);
            Assert.Equal(obj, results);
        }

        [FactWithName]
        public void ExecuteJTokenParams()
        {
            var obj = new IQueryableType2();
            JToken objToken = new JObject
            {
                ["Id"] = 15
            };

            IDictionary<string, object> usedParams = null;

            var resource = new Mock<IOperationResource>();
            resource.Setup(c => c.ExecuteProc(It.IsAny<IDictionary<string, object>>()))
                .Callback<IDictionary<string, object>>(c => usedParams = c)
               .Returns(obj);


            resourceFactoryMock.Setup(c => c.GetResource("conn1",OperationType.read, "proc1"))
              .Returns(resource.Object);

            var results = storedProcedureOperations.Execute("conn1", "proc1", objToken);
            Assert.Equal(0, loggingService.LogCount);
            Assert.Equal(obj, results);
            Assert.Equal("15", usedParams["Id"].ToString());
        }

        [FactWithName]
        public void ExecuteParams()
        {
            var obj = new IQueryableType2();
            var objParams = new IQueryableType() { Id = 34 };

            IDictionary<string, object> usedParams = null;

            var resource = new Mock<IOperationResource>();
            resource.Setup(c => c.ExecuteProc(It.IsAny<IDictionary<string, object>>()))
                .Callback<IDictionary<string, object>>(c => usedParams = c)
               .Returns(obj);


            resourceFactoryMock.Setup(c => c.GetResource("conn1",OperationType.read, "proc1"))
              .Returns(resource.Object);

            var results = storedProcedureOperations.Execute("conn1", "proc1", objParams);
            Assert.Equal(0, loggingService.LogCount);
            Assert.Equal(obj, results);
            Assert.Equal("34", usedParams["Id"].ToString());
        }

        [FactWithName]
        public void ProcDefinition()
        {
            var definition = new StoredProcedureDefinition { Name = "returned" };

            var resource = new Mock<IOperationResource>();
            resource.Setup(c => c.GetStoredProcedureDefinition())
               .Returns(definition);

            resourceFactoryMock.Setup(c => c.GetResource("conn1", OperationType.Any, "proc1"))
            .Returns(resource.Object);

            var result = storedProcedureOperations.Definition("conn1", "proc1");
            Assert.Equal(0, loggingService.LogCount);
            Assert.Equal(definition, result);
        }

        [FactWithName]
        public void ProcParamDefinition()
        {
            var definition = new StoredProcedureParameterDefinition { Name = "returned" };
            var resource = new Mock<IOperationResource>();
            resource.Setup(c => c.GetStoredProcedureDefinition("name"))
               .Returns(definition);

            resourceFactoryMock.Setup(c => c.GetResource("conn1", OperationType.Any, "proc1"))
            .Returns(resource.Object);

            var result = storedProcedureOperations.Definition("conn1", "proc1", "name");
            Assert.Equal(0, loggingService.LogCount);
            Assert.Equal(definition, result);
        }
    }
}
