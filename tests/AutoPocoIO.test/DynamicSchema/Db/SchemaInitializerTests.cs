using AutoPocoIO.DynamicSchema.Db;
using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.Models;
using AutoPocoIO.test;
using Xunit;
using Moq;

namespace AutoPocoIO.Auth.test.DynamicSchema
{
    
     [Trait("Category", TestCategories.Unit)]
    public class SchemaInitializerTests
    {
        [FactWithName]
        public void ConfgiureAction()
        {
            var config = new Config();

            var schemaBuilder = new Mock<IDbSchemaBuilder>();

            var schmea = new SchemaInitializer(config,
             schemaBuilder.Object);

            schmea.ConfigureAction(new Connector { ConnectionStringDecrypted = "conn1" }, OperationType.read);
            Assert.Equal("conn1", config.ConnectionString);
        }

        [FactWithName]
        public void InitializeTable()
        {
            var config = new Config
            {
                IncludedTable = "tbl"
            };

            var schemaBuilder = new Mock<IDbSchemaBuilder>();

            var schmea = new SchemaInitializer(config,
             schemaBuilder.Object);

            schmea.Initilize();

            schemaBuilder.Verify(c => c.GetColumns(), Times.Once);
        }

        [FactWithName]
        public void InitializeSchema()
        {
            var config = new Config();

            var schemaBuilder = new Mock<IDbSchemaBuilder>();

            var schmea = new SchemaInitializer(config,
             schemaBuilder.Object);
            
            schmea.Initilize();

            schemaBuilder.Verify(c => c.GetTableViews(), Times.Once);
            schemaBuilder.Verify(c => c.GetStoredProcedures(), Times.Once);
        }
    }
}
