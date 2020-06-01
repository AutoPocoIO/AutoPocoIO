using AutoPocoIO.Api;
using AutoPocoIO.Models;
using AutoPocoIO.WebApi;
using Xunit;

namespace AutoPocoIO.test.WebApi
{
    
     [Trait("Category", TestCategories.Unit)]
    public class StoredProcedureParameterDefinitionTests : WebApiTestBase<IStoredProcedureOperations>
    {
        [FactWithName]
        public void GetDefinition()
        {
            var obj = new StoredProcedureDefinition { Name = "proc" };

            Ops.Setup(c => c.Definition("conn", "proc", LoggingService))
                .Returns(obj);

            var controller = new StoredProcedureDefinitionController(Ops.Object, LoggingService);

            var results = controller.Get("conn", "proc");
            Assert.Equal("proc", obj.Name);
        }

        [FactWithName]
        public void GetParameterDefinition()
        {
            var obj = new StoredProcedureParameterDefinition { Name = "param1" };

            Ops.Setup(c => c.Definition("conn", "proc", "param1", LoggingService))
                .Returns(obj);

            var controller = new StoredProcedureDefinitionController(Ops.Object, LoggingService);

            var results = controller.Get("conn", "proc", "param1");
            Assert.Equal("param1", obj.Name);
        }
    }
}
