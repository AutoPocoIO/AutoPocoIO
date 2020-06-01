using AutoPocoIO.Api;
using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.Resources;
using Xunit;
using Moq;
using System.Collections.Generic;
using System.Linq;


namespace AutoPocoIO.test.Api
{
    
     [Trait("Category", TestCategories.Unit)]
    public class ViewOperationsNoLoggingTests : ApiOperationBase
    {
        private readonly ViewOperations viewOperations;

        public ViewOperationsNoLoggingTests()
        {
            viewOperations = new ViewOperations(serviceProvider);
        }

        [FactWithName]
        public void GetAll()
        {
            var resource = new Mock<IOperationResource>();
            resource.SetupGet(c => c.Connector)
                .Returns(new Models.Connector { RecordLimit = 54 });

            resource.Setup(c => c.GetViewRecords())
                .Returns(iqueryable);

            resourceFactoryMock.Setup(c => c.GetResource("conn1",OperationType.read, "view1"))
                .Returns(resource.Object);

            var (list, recordLimit) = viewOperations.GetAllAndRecordLimit("conn1", "view1");
            Assert.Equal(0, loggingService.LogCount);
            Assert.Equal(typeof(IQueryableType), list.ElementType);
            Assert.IsAssignableFrom<IQueryable<object>>(list);
            Assert.Equal(54, recordLimit);

        }

        [FactWithName]
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

            var results = viewOperations.GetAll<IQueryableType>("conn1", "view1T");
            Assert.Equal(0, loggingService.LogCount);
            Assert.Equal(typeof(IQueryableType), results.ElementType);
            Assert.IsAssignableFrom<IQueryable<object>>(results);
            Assert.IsAssignableFrom<IQueryable<IQueryableType>>(results);

        }
    }
}
