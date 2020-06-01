using AutoPocoIO.Api;
using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.Models;
using AutoPocoIO.Resources;
using Xunit;
using Moq;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;


namespace AutoPocoIO.test.Api
{
    public class IQueryableTypeOneToOne { public int Id { get; set; } public IQueryableType OneToOne { get; set; } }
    public class IQueryableType2 { public int Id { get; set; } public IEnumerable<int> IntList { get; set; } }
    public class IQueryableType { public int Id { get; set; } }

    
     [Trait("Category", TestCategories.Unit)]
    public class TableOperationsNoLoggingTests : ApiOperationBase
    {
        private readonly TableOperations tableOperations;

        public TableOperationsNoLoggingTests()
        {
            tableOperations = new TableOperations(serviceProvider);
        }

        [FactWithName]
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
            Assert.Equal(0, loggingService.LogCount);
            Assert.Equal(typeof(IQueryableType), list.ElementType);
            Assert.IsAssignableFrom<IQueryable<object>>(list);
            Assert.Equal(54, connectorMax);

        }

        [FactWithName]
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
            Assert.Equal(0, loggingService.LogCount);
            Assert.Equal(typeof(IQueryableType), results.ElementType);
            Assert.IsAssignableFrom<IQueryable<object>>(results);
            Assert.IsAssignableFrom<IQueryable<IQueryableType>>(results);

        }

        [FactWithName]
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
            Assert.Equal(1, results.Count());
            Assert.Single(results.First().IntList);
        }


        [FactWithName]
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
            Assert.Equal(1, results.Count());
            Assert.Equal(1, results.First().OneToOne.Id);
        }

        [FactWithName]
        public void GetById()
        {
            var resultsList = new IQueryableType { Id = 1 };

            var resource = new Mock<IOperationResource>();
            resource.Setup(c => c.GetResourceRecordById("1"))
                .Returns(resultsList);

            resourceFactoryMock.Setup(c => c.GetResource("conn1", OperationType.read, "table1"))
                .Returns(resource.Object);

            var results = tableOperations.GetById("conn1", "table1", "1");
            Assert.Equal(0, loggingService.LogCount);

            Assert.Equal(1, ((IQueryableType)results).Id);
            Assert.IsAssignableFrom<object>(results);
            Assert.IsType<IQueryableType>(results);
        }

        [FactWithName]
        public void GetByIdT()
        {
            var resultsList = new IQueryableType { Id = 1 };

            var resource = new Mock<IOperationResource>();
            resource.Setup(c => c.GetResourceRecordById("1"))
                .Returns(resultsList);

            resourceFactoryMock.Setup(c => c.GetResource("conn1", OperationType.read, "table1"))
                .Returns(resource.Object);

            var results = tableOperations.GetById<IQueryableType>("conn1", "table1", "1");

            Assert.Equal(1, results.Id);
            Assert.IsType<IQueryableType>(results);
        }

        [FactWithName]
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
            Assert.Equal(0, loggingService.LogCount);
            Assert.Equal(15, ((IQueryableType)result).Id);
        }

        [FactWithName]
        public void CreateNewRowT()
        {
            var objT = new IQueryableType { Id = 15 };

            var resource = new Mock<IOperationResource>();
            resource.Setup(c => c.CreateNewResourceRecord(objT))
                .Returns(objT);

            resourceFactoryMock.Setup(c => c.GetResource("conn1", OperationType.write, "table1"))
                .Returns(resource.Object);

            var result = tableOperations.CreateNewRow("conn1", "table1", objT);
            Assert.Equal(0, loggingService.LogCount);
            Assert.Equal(15, result.Id);
        }

        [FactWithName]
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

            var result = tableOperations.UpdateRow("conn1", "table1", "15", obj);
            Assert.Equal(0, loggingService.LogCount);
            Assert.Equal(15, ((IQueryableType)result).Id);
        }

        [FactWithName]
        public void UpdateRowT()
        {
            var objT = new IQueryableType { Id = 15 };

            var resource = new Mock<IOperationResource>();
            resource.Setup(c => c.UpdateResourceRecordById(objT, "15"))
                .Returns(objT);

            resourceFactoryMock.Setup(c => c.GetResource("conn1", OperationType.write, "table1"))
                .Returns(resource.Object);

            var result = tableOperations.UpdateRow("conn1", "table1", "15", objT);
            Assert.Equal(0, loggingService.LogCount);
            Assert.Equal(15, result.Id);
        }

        [FactWithName]
        public void DeleteRow()
        {
            var objT = new IQueryableType { Id = 15 };

            var resource = new Mock<IOperationResource>();
            resource.Setup(c => c.DeleteResourceRecordById("15"))
                .Returns(objT);

            resourceFactoryMock.Setup(c => c.GetResource("conn1", OperationType.delete, "table1"))
                .Returns(resource.Object);

            var result = tableOperations.DeleteRow("conn1", "table1", "15");
            Assert.Equal(0, loggingService.LogCount);
            Assert.Equal(15, ((IQueryableType)result).Id);
        }

        [FactWithName]
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
            Assert.Equal(0, loggingService.LogCount);
            Assert.Equal(definition, result);
        }

        [FactWithName]
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
            Assert.Equal(0, loggingService.LogCount);
            Assert.Equal(definition, result);
        }


    }
}
