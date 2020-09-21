
using AutoPocoIO.CustomAttributes;
using AutoPocoIO.SwaggerAddons;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AutoPocoIO.AspNetCore.test.Swagger
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class ODataParametersDefinitionTests
    {
        readonly ODataParametersSwaggerDefinition swaggerDef = new ODataParametersSwaggerDefinition();
        OpenApiOperation op = new OpenApiOperation { Parameters = null };
        ApiDescription api;
        MethodInfo oDataAction;
        OperationFilterContext context;

        [TestInitialize]
        public void Init()
        {
            api = new ApiDescription();

            var mockMethodInfo = new Mock<MethodInfo>();
            mockMethodInfo.Setup(c => c.GetCustomAttributes(false))
                .Returns(new[] { new UseOdataInSwaggerAttribute() });

            oDataAction = mockMethodInfo.Object;

            context = new OperationFilterContext(api, Mock.Of<ISchemaGenerator>(), Mock.Of<SchemaRepository>(), Mock.Of<MethodInfo>());
        }

        [TestMethod]
        public void SkipOdataParamsIfMissingAttr()
        {
            op = new OpenApiOperation { Parameters = new List<OpenApiParameter> { new OpenApiParameter { Name = "orginial" } } };
            swaggerDef.Apply(op, context);

            Assert.AreEqual(1, op.Parameters.Count());
        }

        [TestMethod]
        public void AppendOdataOpsIfSomeAlreadyExists()
        {
            op = new OpenApiOperation { Parameters = new List<OpenApiParameter> { new OpenApiParameter { Name = "orginial" } } };
            context = new OperationFilterContext(api, Mock.Of<ISchemaGenerator>(), Mock.Of<SchemaRepository>(), oDataAction);
            swaggerDef.Apply(op, context);


            Assert.AreEqual(9, op.Parameters.Count());
        }

        [TestMethod]
        public void SetToOdataOpsIfNoParamsAlreadyExists()
        {
            context = new OperationFilterContext(api, Mock.Of<ISchemaGenerator>(), Mock.Of<SchemaRepository>(), oDataAction);
            swaggerDef.Apply(op, context);

            Assert.AreEqual(8, op.Parameters.Count());
        }

        [TestMethod]
        public void UseOdataAddsSelectParameter()
        {
            context = new OperationFilterContext(api, Mock.Of<ISchemaGenerator>(), Mock.Of<SchemaRepository>(), oDataAction);
            swaggerDef.Apply(op, context);

            var odataOp = (OpenApiParameter)op.Parameters.First(c => c.Name == "$select");

            Assert.AreEqual(ParameterLocation.Query, odataOp.In);
            Assert.AreEqual(false, odataOp.Required);
            Assert.AreEqual("Select columns using OData syntax.", odataOp.Description);
        }

        [TestMethod]
        public void UseOdataAddsExpandParameter()
        {
            context = new OperationFilterContext(api, Mock.Of<ISchemaGenerator>(), Mock.Of<SchemaRepository>(), oDataAction);
            swaggerDef.Apply(op, context);

            var odataOp = (OpenApiParameter)op.Parameters.First(c => c.Name == "$expand");

            Assert.AreEqual(ParameterLocation.Query, odataOp.In);
            Assert.AreEqual(false, odataOp.Required);
            Assert.AreEqual("Expand nested data using OData syntax.", odataOp.Description);
        }

        [TestMethod]
        public void UseOdataAddsOrderByParameter()
        {
            context = new OperationFilterContext(api, Mock.Of<ISchemaGenerator>(), Mock.Of<SchemaRepository>(), oDataAction);
            swaggerDef.Apply(op, context);

            var odataOp = (OpenApiParameter)op.Parameters.First(c => c.Name == "$orderby");

            Assert.AreEqual(ParameterLocation.Query, odataOp.In);
            Assert.AreEqual(false, odataOp.Required);
            Assert.AreEqual("Order the results using OData syntax.", odataOp.Description);
        }

        [TestMethod]
        public void UseOdataAddsSkipParameter()
        {
            context = new OperationFilterContext(api, Mock.Of<ISchemaGenerator>(), Mock.Of<SchemaRepository>(), oDataAction);
            swaggerDef.Apply(op, context);

            var odataOp = (OpenApiParameter)op.Parameters.First(c => c.Name == "$skip");

            Assert.AreEqual(ParameterLocation.Query, odataOp.In);
            Assert.AreEqual(false, odataOp.Required);
            Assert.AreEqual("The number of results to skip.", odataOp.Description);
        }

        [TestMethod]
        public void UseOdataAddsTopParameter()
        {
            context = new OperationFilterContext(api, Mock.Of<ISchemaGenerator>(), Mock.Of<SchemaRepository>(), oDataAction);
            swaggerDef.Apply(op, context);

            var odataOp = (OpenApiParameter)op.Parameters.First(c => c.Name == "$top");

            Assert.AreEqual(ParameterLocation.Query, odataOp.In);
            Assert.AreEqual(false, odataOp.Required);
            Assert.AreEqual("The number of results to return.", odataOp.Description);
        }

        [TestMethod]
        public void UseOdataAddsApplyParameter()
        {
            context = new OperationFilterContext(api, Mock.Of<ISchemaGenerator>(), Mock.Of<SchemaRepository>(), oDataAction);
            swaggerDef.Apply(op, context);

            var odataOp = (OpenApiParameter)op.Parameters.First(c => c.Name == "$apply");

            Assert.AreEqual(ParameterLocation.Query, odataOp.In);
            Assert.AreEqual(false, odataOp.Required);
            Assert.AreEqual("Return applied filter.", odataOp.Description);
        }

        [TestMethod]
        public void UseOdataAddsCountParameter()
        {
            context = new OperationFilterContext(api, Mock.Of<ISchemaGenerator>(), Mock.Of<SchemaRepository>(), oDataAction);
            swaggerDef.Apply(op, context);

            var odataOp = (OpenApiParameter)op.Parameters.First(c => c.Name == "$count");

            Assert.AreEqual(ParameterLocation.Query, odataOp.In);
            Assert.AreEqual(false, odataOp.Required);
            Assert.AreEqual("Return the total count.", odataOp.Description);
        }
    }
}
