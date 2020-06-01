using AutoPocoIO.Api;
using AutoPocoIO.Models;
using AutoPocoIO.WebApi;
using Xunit;

namespace AutoPocoIO.test.WebApi
{
    
     [Trait("Category", TestCategories.Unit)]
    public class TableDefinitionControllerTests : WebApiTestBase<ITableOperations>
    {
        [FactWithName]
        public void GetDefinition()
        {
            var obj = new TableDefinition { Name = "tbl" };

            Ops.Setup(c => c.Definition("conn", "tbl", LoggingService))
                .Returns(obj);

            var controller = new TableDefinitionController(Ops.Object, LoggingService);

            var results = controller.Get("conn", "tbl");
            Assert.Equal("tbl", obj.Name);
        }

        [FactWithName]
        public void GetColumnDefinition()
        {
            var obj = new ColumnDefinition { Name = "col1" };

            Ops.Setup(c => c.Definition("conn", "tbl", "col1", LoggingService))
                .Returns(obj);

            var controller = new TableDefinitionController(Ops.Object, LoggingService);

            var results = controller.Get("conn", "tbl", "col1");
            Assert.Equal("col1", obj.Name);
        }
    }
}
