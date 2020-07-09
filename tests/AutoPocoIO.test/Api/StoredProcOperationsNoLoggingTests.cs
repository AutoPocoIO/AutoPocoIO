﻿using AutoPocoIO.Api;
using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.Models;
using AutoPocoIO.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace AutoPocoIO.test.Api
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class StoredProcOperationsNoLoggingTests : ApiOperationBase
    {
        private StoredProcedureOperations storedProcedureOperations;
        [TestInitialize]
        public void InitOperation()
        {
            storedProcedureOperations = new StoredProcedureOperations(resourceFactoryMock.Object);
        }

        [TestMethod]
        public void ExecuteNoParams()
        {
            var obj = new Dictionary<string, object>();

            var resource = new Mock<IOperationResource>();
            resource.Setup(c => c.ExecuteProc(new Dictionary<string, object>()))
               .Returns(obj);

            resourceFactoryMock.Setup(c => c.GetResource("conn1",OperationType.read, "proc1"))
                .Returns(resource.Object);

            var results = storedProcedureOperations.ExecuteNoParameters("conn1", "proc1");
            Assert.AreEqual(0, loggingService.LogCount);
            Assert.AreEqual(obj, results);
        }

        [TestMethod]
        public void ExecuteJTokenParams()
        {
            var obj = new Dictionary<string, object>();
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
            Assert.AreEqual(0, loggingService.LogCount);
            Assert.AreEqual(obj, results);
            Assert.AreEqual("15", usedParams["Id"].ToString());
        }

        [TestMethod]
        public void ExecuteParams()
        {
            var obj = new Dictionary<string, object>();
            var objParams = new IQueryableType() { Id = 34 };

            IDictionary<string, object> usedParams = null;

            var resource = new Mock<IOperationResource>();
            resource.Setup(c => c.ExecuteProc(It.IsAny<IDictionary<string, object>>()))
                .Callback<IDictionary<string, object>>(c => usedParams = c)
               .Returns(obj);


            resourceFactoryMock.Setup(c => c.GetResource("conn1",OperationType.read, "proc1"))
              .Returns(resource.Object);

            var results = storedProcedureOperations.Execute("conn1", "proc1", objParams);
            Assert.AreEqual(0, loggingService.LogCount);
            Assert.AreEqual(obj, results);
            Assert.AreEqual("34", usedParams["Id"].ToString());
        }

        [TestMethod]
        public void ProcDefinition()
        {
            var definition = new StoredProcedureDefinition { Name = "returned" };

            var resource = new Mock<IOperationResource>();
            resource.Setup(c => c.GetStoredProcedureDefinition())
               .Returns(definition);

            resourceFactoryMock.Setup(c => c.GetResource("conn1", OperationType.Any, "proc1"))
            .Returns(resource.Object);

            var result = storedProcedureOperations.Definition("conn1", "proc1");
            Assert.AreEqual(0, loggingService.LogCount);
            Assert.AreEqual(definition, result);
        }

        [TestMethod]
        public void ProcParamDefinition()
        {
            var definition = new StoredProcedureParameterDefinition { Name = "returned" };
            var resource = new Mock<IOperationResource>();
            resource.Setup(c => c.GetStoredProcedureDefinition("name"))
               .Returns(definition);

            resourceFactoryMock.Setup(c => c.GetResource("conn1", OperationType.Any, "proc1"))
            .Returns(resource.Object);

            var result = storedProcedureOperations.Definition("conn1", "proc1", "name");
            Assert.AreEqual(0, loggingService.LogCount);
            Assert.AreEqual(definition, result);
        }
    }
}
