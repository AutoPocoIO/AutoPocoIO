using AutoPocoIO.Api;
using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;


namespace AutoPocoIO.test.Api
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class ViewOperationsWithLogging : ApiOperationBase
    {
        private ViewOperations viewOperations;

        [TestInitialize]
        public void InitOperation()
        {
            viewOperations = new ViewOperations(serviceProvider);
        }

        [TestMethod]
        public void GetAll()
        {
            var resource = new Mock<IOperationResource>();
            resource.SetupGet(c => c.Connector)
                .Returns(new Models.Connector { RecordLimit = 54 });

            resource.Setup(c => c.GetViewRecords())
                .Returns(iqueryable);

            resourceFactoryMock.Setup(c => c.GetResource("conn1",OperationType.read, "view1"))
                .Returns(resource.Object);

            var (list, recordLimit) = viewOperations.GetAllAndRecordLimit("conn1", "view1", loggingService);
            Assert.AreEqual(1, loggingService.LogCount);
            Assert.AreEqual("GET", loggingService.ApiRequests.First().RequestType);
            Assert.AreEqual(typeof(IQueryableType), list.ElementType);
            Assert.IsInstanceOfType(list, typeof(IQueryable<object>));
            Assert.IsNotInstanceOfType(list, typeof(IQueryable<IQueryableType>));
            Assert.AreEqual(54, recordLimit);

        }

        [TestMethod]
        public void GetAllT()
        {
            var resultsList = new List<IQueryableType>
            {
                new IQueryableType{ Id = 1 }
            }.AsQueryable();

            var resource = new Mock<IOperationResource>();
            resource.Setup(c => c.GetViewRecords())
                .Returns(resultsList);

            resourceFactoryMock.Setup(c => c.GetResource("conn1",OperationType.read, "view1T"))
                .Returns(resource.Object);

            var results = viewOperations.GetAll<IQueryableType>("conn1", "view1T", loggingService);
            Assert.AreEqual(1, loggingService.LogCount);
            Assert.AreEqual("GET", loggingService.ApiRequests.First().RequestType);
            Assert.AreEqual(typeof(IQueryableType), results.ElementType);
            Assert.IsInstanceOfType(results, typeof(IQueryable<object>));
            Assert.IsInstanceOfType(results, typeof(IQueryable<IQueryableType>));

        }
    }
}
