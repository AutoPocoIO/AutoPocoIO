using AutoPocoIO.Api;
using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.Models;
using AutoPocoIO.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;


namespace AutoPocoIO.test.Api
{
    public class IQueryableTypeOneToOne { public int Id { get; set; } public IQueryableType OneToOne { get; set; } }
    public class IQueryableType2 { public int Id { get; set; } public IEnumerable<int> IntList { get; set; } }
    public class IQueryableType { public int Id { get; set; } }

    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class TableOperationsNoLoggingTests : ApiOperationBase
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

            resourceFactoryMock.Setup(c => c.GetResource("conn1", OperationType.read, "table1"))
                .Returns(resource.Object);

            var (list, connectorMax) = tableOperations.GetAll("conn1", "table1");
            Assert.AreEqual(0, loggingService.LogCount);
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

            resourceFactoryMock.Setup(c => c.GetResource("conn1", OperationType.read, "table1T"))
                .Returns(resource.Object);

            var results = tableOperations.GetAll<IQueryableType>("conn1", "table1T");
            Assert.AreEqual(0, loggingService.LogCount);
            Assert.AreEqual(typeof(IQueryableType), results.ElementType);
            Assert.IsInstanceOfType(results, typeof(IQueryable<object>));
            Assert.IsInstanceOfType(results, typeof(IQueryable<IQueryableType>));

        }

        [TestMethod]
        public void GetAllTVerifyExpand()
        {
            var resultsList = new List<IQueryableType2>
            {
                new IQueryableType2
                {
                    Id = 1,
                    IntList = new List<int>
                    {
                       2
                    }
                }
            }.AsQueryable();

            var resource = new Mock<IOperationResource>();
            resource.Setup(c => c.LoadDbAdapter()).Verifiable();
            resource.Setup(c => c.GetResourceRecords(new Dictionary<string, string>() { { "$expand", "IntList" } }))
                .Returns(resultsList);

            resourceFactoryMock.Setup(c => c.GetResource("conn1", OperationType.read, "table1TList"))
                .Returns(resource.Object);

            var results = tableOperations.GetAll<IQueryableType2>("conn1", "table1TList");

            //Verify child lists are shown
            Assert.AreEqual(1, results.Count());
            Assert.AreEqual(1, results.First().IntList.Count());
        }


        [TestMethod]
        public void GetAllTVerifyExpandOneToOne()
        {
            var resultsList = new List<IQueryableTypeOneToOne>
            {
                new IQueryableTypeOneToOne
                {
                    Id = 3,
                    OneToOne = new IQueryableType
                    {
                        Id = 1,
                    }
                }
            }.AsQueryable();

            var resource = new Mock<IOperationResource>();
            resource.Setup(c => c.LoadDbAdapter()).Verifiable();
            resource.Setup(c => c.GetResourceRecords(new Dictionary<string, string>() { { "$expand", "OneToOne" } }))
                .Returns(resultsList);

            resourceFactoryMock.Setup(c => c.GetResource("conn1", OperationType.read, "table1TList"))
                .Returns(resource.Object);

            var results = tableOperations.GetAll<IQueryableTypeOneToOne>("conn1", "table1TList");

            //Verify child lists are shown
            Assert.AreEqual(1, results.Count());
            Assert.AreEqual(1, results.First().OneToOne.Id);
        }

        [TestMethod]
        public void GetById()
        {
            var resultsList = new IQueryableType { Id = 1 };

            var resource = new Mock<IOperationResource>();
            resource.Setup(c => c.GetResourceRecordById("1"))
                .Returns(resultsList);

            resourceFactoryMock.Setup(c => c.GetResource("conn1", OperationType.read, "table1"))
                .Returns(resource.Object);

            var results = tableOperations.GetById("conn1", "table1", "1");
            Assert.AreEqual(0, loggingService.LogCount);

            Assert.AreEqual(1, ((IQueryableType)results).Id);
            Assert.IsInstanceOfType(results, typeof(object));
            Assert.IsInstanceOfType(results, typeof(IQueryableType));
        }

        [TestMethod]
        public void GetByIdT()
        {
            var resultsList = new IQueryableType { Id = 1 };

            var resource = new Mock<IOperationResource>();
            resource.Setup(c => c.GetResourceRecordById<IQueryableType>("1", new Dictionary<string, string>()))
                .Returns(resultsList);

            resourceFactoryMock.Setup(c => c.GetResource("conn1", OperationType.read, "table1"))
                .Returns(resource.Object);

            var results = tableOperations.GetById<IQueryableType>("conn1", "table1", "1");

            Assert.AreEqual(1, results.Id);
            Assert.IsInstanceOfType(results, typeof(IQueryableType));
        }

        [TestMethod]
        public void GetByIdTVerifyExpand()
        {
            var result =
                new IQueryableType2
                {
                    Id = 1,
                    IntList = new List<int>
                    {
                       2
                    }
                };

            var resource = new Mock<IOperationResource>();
            resource.Setup(c => c.LoadDbAdapter()).Verifiable();
            resource.Setup(c => c.GetResourceRecordById<IQueryableType2>("1", new Dictionary<string, string>() { { "$expand", "IntList" } }))
                .Returns(result);

            resourceFactoryMock.Setup(c => c.GetResource("conn1", OperationType.read, "table1TList"))
                .Returns(resource.Object);

            var results = tableOperations.GetById<IQueryableType2>("conn1", "table1TList", "1");

            //Verify child lists are shown
            Assert.AreEqual(1, results.IntList.Count());
        }


        [TestMethod]
        public void GetByIdTVerifyExpandOneToOne()
        {
            var result = new IQueryableTypeOneToOne
            {
                Id = 3,
                OneToOne = new IQueryableType
                {
                    Id = 1,
                }
            };


            var resource = new Mock<IOperationResource>();
            resource.Setup(c => c.LoadDbAdapter()).Verifiable();
            resource.Setup(c => c.GetResourceRecordById<IQueryableTypeOneToOne>("3", new Dictionary<string, string>() { { "$expand", "OneToOne" } }))
                .Returns(result);

            resourceFactoryMock.Setup(c => c.GetResource("conn1", OperationType.read, "table1TList"))
                .Returns(resource.Object);

            var results = tableOperations.GetById<IQueryableTypeOneToOne>("conn1", "table1TList", "3");

            //Verify child object are shown
            Assert.AreEqual(1, results.OneToOne.Id);
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

            var result = tableOperations.CreateNewRow("conn1", "table1", obj);
            Assert.AreEqual(0, loggingService.LogCount);
            Assert.AreEqual(15, ((IQueryableType)result).Id);
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

            var result = tableOperations.CreateNewRow("conn1", "table1", objT);
            Assert.AreEqual(0, loggingService.LogCount);
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
            resource.Setup(c => c.GetPrimaryKeys(obj)).Returns("15");
            resource.Setup(c => c.UpdateResourceRecordById(obj, "15"))
                .Returns(objT);

            resourceFactoryMock.Setup(c => c.GetResource("conn1", OperationType.write, "table1"))
                .Returns(resource.Object);

            var result = tableOperations.UpdateRow("conn1", "table1", obj);
            Assert.AreEqual(0, loggingService.LogCount);
            Assert.AreEqual(15, ((IQueryableType)result).Id);
        }

        [TestMethod]
        public void UpdateRowT()
        {
            var objT = new IQueryableType { Id = 15 };

            var resource = new Mock<IOperationResource>();
            resource.Setup(c => c.GetPrimaryKeys(objT)).Returns("15");
            resource.Setup(c => c.UpdateResourceRecordById(objT, "15"))
                .Returns(objT);

            resourceFactoryMock.Setup(c => c.GetResource("conn1", OperationType.write, "table1"))
                .Returns(resource.Object);

            var result = tableOperations.UpdateRow("conn1", "table1", objT);
            Assert.AreEqual(0, loggingService.LogCount);
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

            var result = tableOperations.DeleteRow("conn1", "table1", "15");
            Assert.AreEqual(0, loggingService.LogCount);
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

            var result = tableOperations.Definition("conn1", "table1");
            Assert.AreEqual(0, loggingService.LogCount);
            Assert.AreEqual(definition, result);
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

            var result = tableOperations.Definition("conn1", "table1", "col12");
            Assert.AreEqual(0, loggingService.LogCount);
            Assert.AreEqual(definition, result);
        }


    }
}
