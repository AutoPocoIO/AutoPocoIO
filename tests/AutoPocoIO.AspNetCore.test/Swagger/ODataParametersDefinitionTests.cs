using AutoPocoIO.CustomAttributes;
using AutoPocoIO.SwaggerAddons;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Moq;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace AutoPocoIO.AspNetCore.test.Swagger
{

    [Trait("Category", TestCategories.Unit)]
    public class ODataParametersDefinitionTests
    {
        readonly ODataParametersSwaggerDefinition swaggerDef = new ODataParametersSwaggerDefinition();
        Operation op = new Operation { Parameters = null };
        readonly ApiDescription api;
        readonly MethodInfo oDataAction;
        OperationFilterContext context;

        public ODataParametersDefinitionTests()
        {
            api = new ApiDescription();

            var mockMethodInfo = new Mock<MethodInfo>();
            mockMethodInfo.Setup(c => c.GetCustomAttributes(false))
                .Returns(new[] { new UseOdataInSwaggerAttribute() });

            oDataAction = mockMethodInfo.Object;

            context = new OperationFilterContext(api, Mock.Of<ISchemaRegistry>(), Mock.Of<MethodInfo>());
        }

        [FactWithName]
        public void SkipOdataParamsIfMissingAttr()
        {
            op = new Operation { Parameters = new List<IParameter> { new NonBodyParameter { Name = "orginial" } } };
            swaggerDef.Apply(op, context);

            Assert.Single(op.Parameters);
        }

        [FactWithName]
        public void AppendOdataOpsIfSomeAlreadyExists()
        {
            op = new Operation { Parameters = new List<IParameter> { new NonBodyParameter { Name = "orginial" } } };
            context = new OperationFilterContext(api, Mock.Of<ISchemaRegistry>(), oDataAction);
            swaggerDef.Apply(op, context);


            Assert.Equal(9, op.Parameters.Count());
        }

        [FactWithName]
        public void SetToOdataOpsIfNoParamsAlreadyExists()
        {
            context = new OperationFilterContext(api, Mock.Of<ISchemaRegistry>(), oDataAction);
            swaggerDef.Apply(op, context);

            Assert.Equal(8, op.Parameters.Count());
        }

        [FactWithName]
        public void UseOdataAddsSelectParameter()
        {
            context = new OperationFilterContext(api, Mock.Of<ISchemaRegistry>(), oDataAction);
            swaggerDef.Apply(op, context);

            var odataOp = (NonBodyParameter)op.Parameters.First(c => c.Name == "$select");

            Assert.Equal("string", odataOp.Type);
            Assert.Equal("query", odataOp.In);
            Assert.False(odataOp.Required);
            Assert.Equal("Select columns using OData syntax.", odataOp.Description);
        }

        [FactWithName]
        public void UseOdataAddsExpandParameter()
        {
            context = new OperationFilterContext(api, Mock.Of<ISchemaRegistry>(), oDataAction);
            swaggerDef.Apply(op, context);

            var odataOp = (NonBodyParameter)op.Parameters.First(c => c.Name == "$expand");

            Assert.Equal("string", odataOp.Type);
            Assert.Equal("query", odataOp.In);
            Assert.False(odataOp.Required);
            Assert.Equal("Expand nested data using OData syntax.", odataOp.Description);
        }

        [FactWithName]
        public void UseOdataAddsOrderByParameter()
        {
            context = new OperationFilterContext(api, Mock.Of<ISchemaRegistry>(), oDataAction);
            swaggerDef.Apply(op, context);

            var odataOp = (NonBodyParameter)op.Parameters.First(c => c.Name == "$orderby");

            Assert.Equal("string", odataOp.Type);
            Assert.Equal("query", odataOp.In);
            Assert.False(odataOp.Required);
            Assert.Equal("Order the results using OData syntax.", odataOp.Description);
        }

        [FactWithName]
        public void UseOdataAddsSkipParameter()
        {
            context = new OperationFilterContext(api, Mock.Of<ISchemaRegistry>(), oDataAction);
            swaggerDef.Apply(op, context);

            var odataOp = (NonBodyParameter)op.Parameters.First(c => c.Name == "$skip");

            Assert.Equal("integer", odataOp.Type);
            Assert.Equal("query", odataOp.In);
            Assert.False(odataOp.Required);
            Assert.Equal("The number of results to skip.", odataOp.Description);
        }

        [FactWithName]
        public void UseOdataAddsTopParameter()
        {
            context = new OperationFilterContext(api, Mock.Of<ISchemaRegistry>(), oDataAction);
            swaggerDef.Apply(op, context);

            var odataOp = (NonBodyParameter)op.Parameters.First(c => c.Name == "$top");

            Assert.Equal("integer", odataOp.Type);
            Assert.Equal("query", odataOp.In);
            Assert.False(odataOp.Required);
            Assert.Equal("The number of results to return.", odataOp.Description);
        }

        [FactWithName]
        public void UseOdataAddsApplyParameter()
        {
            context = new OperationFilterContext(api, Mock.Of<ISchemaRegistry>(), oDataAction);
            swaggerDef.Apply(op, context);

            var odataOp = (NonBodyParameter)op.Parameters.First(c => c.Name == "$apply");

            Assert.Equal("string", odataOp.Type);
            Assert.Equal("query", odataOp.In);
            Assert.False(odataOp.Required);
            Assert.Equal("Return applied filter.", odataOp.Description);
        }

        [FactWithName]
        public void UseOdataAddsCountParameter()
        {
            context = new OperationFilterContext(api, Mock.Of<ISchemaRegistry>(), oDataAction);
            swaggerDef.Apply(op, context);

            var odataOp = (NonBodyParameter)op.Parameters.First(c => c.Name == "$count");

            Assert.Equal("boolean", odataOp.Type);
            Assert.Equal("query", odataOp.In);
            Assert.False(odataOp.Required);
            Assert.Equal("Return the total count.", odataOp.Description);
        }
    }
}
