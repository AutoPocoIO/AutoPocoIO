using AutoPocoIO.Api;
using AutoPocoIO.Models;
using AutoPocoIO.Services;
using AutoPocoIO.WebApi;
using Xunit;
using Moq;

namespace AutoPocoIO.test.WebApi
{
    
     [Trait("Category", TestCategories.Unit)]
    public class SchemaControllerTests
    {
        private readonly ILoggingService _loggingService = new Mock<ILoggingService>().Object;
        private readonly Mock<ISchemaOperations> _ops = new Mock<ISchemaOperations>();


        [FactWithName]
        public void GetDefinition()
        {
            var obj = new SchemaDefinition { Name = "sch1" };

            _ops.Setup(c => c.Definition("conn", _loggingService))
                .Returns(obj);

            var controller = new SchemaController(_ops.Object, _loggingService);

            var results = controller.Get("conn");
            Assert.Equal("sch1", obj.Name);
        }
    }
}
