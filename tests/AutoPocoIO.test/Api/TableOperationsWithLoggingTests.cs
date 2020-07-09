using AutoPocoIO.Api;
using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.Models;
using AutoPocoIO.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoPocoIO.test.Api
{

    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class TableOperationsWithLoggingTests : ApiOperationBase
    {
        private TableOperations tableOperations;
        [TestInitialize]
        public void InitOperation()
        {
            tableOperations = new TableOperations(resourceFactoryMock.Object, queryStringService.Object);
        }

        [TestMethod]
        public void GetAll()
        {
            var resource = new Mock<IOperationResource>();
            resource.SetupGet(c => c.Connector)
                .Returns(new Models.Connector { RecordLimit = 54 });

            resource.Setup(c => c.GetResourceRecords(new Dictionary<string, string>()))
                .Returns(iqueryable);

            resourceFactoryMock.Setup(c => c.GetResource("conn1",OperationType.read, "table1"))
                .Returns(resource.Object);

            var (list, connectorMax) = tableOperations.GetAll("conn1", "table1", loggingService);
            Assert.AreEqual(1, loggingService.LogCount);
            Assert.AreEqual("GET", loggingService.ApiRequests.First().RequestType);

            Assert.AreEqual(typeof(IQueryableType), list.ElementType);
            Assert.IsInstanceOfType(list, typeof(IQueryable<object>));
            Assert.IsNotInstanceOfType(list, typeof(IQueryable<IQueryableType>));
            Assert.AreEqual(54, connectorMax);
        }

        [TestMethod]
        public void GetAllT()
        {
            var resultsList = new List<IQueryableType>
            {
                new IQueryableType{ Id = 1 }
            }.AsQueryable();

            var resource = new Mock<IOperationResource>();
            resource.Setup(c => c.GetResourceRecords(new Dictionary<string, string>()))
                .Returns(resultsList);

            resourceFactoryMock.Setup(c => c.GetResource("conn1",OperationType.read, "table1T"))
                .Returns(resource.Object);

            var results = tableOperations.GetAll<IQueryableType>("conn1", "table1T", loggingService);
            Assert.AreEqual(1, loggingService.LogCount);
            Assert.AreEqual("GET", loggingService.ApiRequests.First().RequestType);

            Assert.AreEqual(typeof(IQueryableType), results.ElementType);
            Assert.IsInstanceOfType(results, typeof(IQueryable<object>));
            Assert.IsInstanceOfType(results, typeof(IQueryable<IQueryableType>));
        }

        [TestMethod]
        public void GetById()
        {
            var resultsList = new IQueryableType { Id = 1 };

            var resource = new Mock<IOperationResource>();
            resource.Setup(c => c.GetResourceRecordById("1"))
                .Returns(resultsList);

            resourceFactoryMock.Setup(c => c.GetResource("conn1",OperationType.read, "table1"))
                .Returns(resource.Object);

            var results = tableOperations.GetById("conn1", "table1", "1", loggingService);
            Assert.AreEqual(1, loggingService.LogCount);
            Assert.AreEqual("GET", loggingService.ApiRequests.First().RequestType);

            Assert.AreEqual(1, ((IQueryableType)results).Id);
            Assert.IsInstanceOfType(results, typeof(object));
            Assert.IsInstanceOfType(results, typeof(IQueryableType));
        }

        [TestMethod]
        public void CreateNewRow()
        {
            JToken obj = new JObject
            {
                ["Id"] = 15
            };

            var objT = new IQueryableType { Id = 15 };

            var resource = new Mock<IOperationResource>();
            resource.Setup(c => c.CreateNewResourceRecord(obj))
                .Returns(objT);

            resourceFactoryMock.Setup(c => c.GetResource("conn1", OperationType.write, "table1"))
                .Returns(resource.Object);

            var result = tableOperations.CreateNewRow("conn1", "table1", obj, loggingService);
            Assert.AreEqual(1, loggingService.LogCount);
            Assert.AreEqual("POST", loggingService.ApiRequests.First().RequestType);
            Assert.AreEqual(15, ((IQueryableType)result).Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateNewRowThrowIfNull()
        {
            JToken obj = null;
            try
            {
                _ = tableOperations.CreateNewRow("conn1", "table1", obj, loggingService);
            }
            catch (Exception ex)
            {
                Assert.AreEqual(1, loggingService.LogCount);
                Assert.AreEqual("POST", loggingService.ApiRequests.First().RequestType);
                throw ex;
            }

            Assert.Fail();
        }

        [TestMethod]
        public void CreateNewRowT()
        {
            var objT = new IQueryableType { Id = 15 };

            var resource = new Mock<IOperationResource>();
            resource.Setup(c => c.CreateNewResourceRecord(objT))
                .Returns(objT);

            resourceFactoryMock.Setup(c => c.GetResource("conn1", OperationType.write, "table1"))
                .Returns(resource.Object);

            var result = tableOperations.CreateNewRow("conn1", "table1", objT, loggingService);
            Assert.AreEqual(1, loggingService.LogCount);
            Assert.AreEqual("POST", loggingService.ApiRequests.First().RequestType);
            Assert.AreEqual(15, result.Id);
        }

        [TestMethod]
        public void UpdateRow()
        {
            JToken obj = new JObject
            {
                ["Id"] = 15
            };

            var objT = new IQueryableType { Id = 15 };

            var resource = new Mock<IOperationResource>();
            resource.Setup(c => c.UpdateResourceRecordById(obj, "15"))
                .Returns(objT);

            resourceFactoryMock.Setup(c => c.GetResource("conn1", OperationType.write, "table1"))
                .Returns(resource.Object);

            var result = tableOperations.UpdateRow("conn1", "table1", "15", obj, loggingService);
            Assert.AreEqual(1, loggingService.LogCount);
            Assert.AreEqual("PUT", loggingService.ApiRequests.First().RequestType);
            Assert.AreEqual(15, ((IQueryableType)result).Id);
        }

        [TestMethod]
        public void UpdateRowT()
        {
            var objT = new IQueryableType { Id = 15 };

            var resource = new Mock<IOperationResource>();
            resource.Setup(c => c.UpdateResourceRecordById(objT, "15"))
                .Returns(objT);

            resourceFactoryMock.Setup(c => c.GetResource("conn1", OperationType.write, "table1"))
                .Returns(resource.Object);

            var result = tableOperations.UpdateRow("conn1", "table1", "15", objT, loggingService);
            Assert.AreEqual(1, loggingService.LogCount);
            Assert.AreEqual("PUT", loggingService.ApiRequests.First().RequestType);
            Assert.AreEqual(15, result.Id);
        }

        [TestMethod]
        public void DeleteRow()
        {
            var objT = new IQueryableType { Id = 15 };

            var resource = new Mock<IOperationResource>();
            resource.Setup(c => c.DeleteResourceRecordById("15"))
                .Returns(objT);

            resourceFactoryMock.Setup(c => c.GetResource("conn1", OperationType.delete, "table1"))
                .Returns(resource.Object);

            var result = tableOperations.DeleteRow("conn1", "table1", "15", loggingService);
            Assert.AreEqual(1, loggingService.LogCount);
            Assert.AreEqual("DELETE", loggingService.ApiRequests.First().RequestType);
            Assert.AreEqual(15, ((IQueryableType)result).Id);
        }

        [TestMethod]
        public void TableDefinition()
        {
            TableDefinition definition = new TableDefinition
            {
                Name = "returnedName"
            };

            var resource = new Mock<IOperationResource>();
            resource.Setup(c => c.GetTableDefinition())
                .Returns(definition);

            resourceFactoryMock.Setup(c => c.GetResource("conn1", OperationType.Any, "table1"))
             .Returns(resource.Object);

            var result = tableOperations.Definition("conn1", "table1", loggingService);
            Assert.AreEqual(1, loggingService.LogCount);
            Assert.AreEqual("GET", loggingService.ApiRequests.First().RequestType);
            Assert.AreEqual("returnedName", definition.Name);
        }


        [TestMethod]
        public void ColumnDefinition()
        {
            ColumnDefinition definition = new ColumnDefinition
            {
                Name = "returnedName"
            };

            var resource = new Mock<IOperationResource>();
            resource.Setup(c => c.GetColumnDefinition("col12"))
                .Returns(definition);

            resourceFactoryMock.Setup(c => c.GetResource("conn1", OperationType.Any, "table1"))
               .Returns(resource.Object);

            var result = tableOperations.Definition("conn1", "table1", "col12", loggingService);
            Assert.AreEqual(1, loggingService.LogCount);
            Assert.AreEqual("GET", loggingService.ApiRequests.First().RequestType);
            Assert.AreEqual("returnedName", definition.Name);
        }
    }
}
