using AutoPocoIO.CustomAttributes;
using AutoPocoIO.SwaggerAddons;
using AutoPocoIO.WebApi;
using Xunit;
using Moq;
using Swashbuckle.Swagger;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Description;

namespace AutoPocoIO.AspNet.test.Swagger
{
    
    [Trait("Category", TestCategories.Unit)]
    public class ODataParametersDefinitionTests
    {
        readonly SchemaRegistry reg = new SchemaRegistry(new Newtonsoft.Json.JsonSerializerSettings(),
            new Dictionary<Type, Func<Schema>>(),
            new List<ISchemaFilter>(),
            new List<IModelFilter>(),
            true,
            c => c.Name,
            true, true, true);


        readonly ODataParametersSwaggerDefinition swaggerDef = new ODataParametersSwaggerDefinition();
        Operation op = new Operation { parameters = null };
        ApiDescription api;
        SwaggerActionDesc oDataAction;

        public class SwaggerActionDesc : HttpActionDescriptor
        {
            public SwaggerActionDesc(HttpControllerDescriptor controllerDescriptor) : base(controllerDescriptor) { }

            public override string ActionName => throw new NotImplementedException();

            public override Type ReturnType => null;

            public override Task<object> ExecuteAsync(HttpControllerContext controllerContext, IDictionary<string, object> arguments, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public override Collection<HttpParameterDescriptor> GetParameters()
            {
                throw new NotImplementedException();
            }
        }

        public ODataParametersDefinitionTests()
        {
            api = new ApiDescription();
            api.ResponseDescription.ResponseType = typeof(string);

            var ctrlDescriptor = new HttpControllerDescriptor() { ControllerType = typeof(TablesController) };
            api.ActionDescriptor = new SwaggerActionDesc(ctrlDescriptor);

            var ctrlDescriptor2 = new HttpControllerDescriptor() { ControllerType = typeof(string) };
            var mockAction = new Mock<SwaggerActionDesc>(ctrlDescriptor2);
            mockAction.Setup(c => c.GetCustomAttributes<UseOdataInSwaggerAttribute>())
                .Returns(new Collection<UseOdataInSwaggerAttribute> { new UseOdataInSwaggerAttribute() });

            oDataAction = mockAction.Object;

        }


        [FactWithName]
        public void DoNothingIfResponseTypeIsNull()
        {
            api.ResponseDescription.ResponseType = null;
            swaggerDef.Apply(op, reg, api);

            Assert.Null(op.parameters);
        }

        [FactWithName]
        public void UseOdataAddsFilterParameter()
        {
            api.ActionDescriptor = oDataAction;
            swaggerDef.Apply(op, reg, api);

            var odataOp = op.parameters.First(c => c.name == "$filter");

            Assert.Equal("string", odataOp.type);
            Assert.Equal("query", odataOp.@in);
            Assert.Equal(false, odataOp.required);
            Assert.Equal("Filter the results using OData syntax.", odataOp.description);
        }

        [FactWithName]
        public void SkipOdataParamsIfMissingAttr()
        {
            op = new Operation { parameters = new List<Parameter> { new Parameter { name = "orginial" } } };
            var ctrlDescriptor2 = new HttpControllerDescriptor() { ControllerType = typeof(string) };
            var mockAction = new Mock<SwaggerActionDesc>(ctrlDescriptor2);
            mockAction.Setup(c => c.GetCustomAttributes<UseOdataInSwaggerAttribute>())
                .Returns(new Collection<UseOdataInSwaggerAttribute> { });

            api.ActionDescriptor = mockAction.Object;
            swaggerDef.Apply(op, reg, api);

            Assert.Single(op.parameters);
        }

        [FactWithName]
        public void AppendOdataOpsIfSomeAlreadyExists()
        {
            op = new Operation { parameters = new List<Parameter> { new Parameter { name = "orginial" } } };
            var ctrlDescriptor2 = new HttpControllerDescriptor() { ControllerType = typeof(string) };
            var mockAction = new Mock<SwaggerActionDesc>(ctrlDescriptor2);
            mockAction.Setup(c => c.GetCustomAttributes<UseOdataInSwaggerAttribute>())
                .Returns(new Collection<UseOdataInSwaggerAttribute> { new UseOdataInSwaggerAttribute() });

            api.ActionDescriptor = mockAction.Object;
            swaggerDef.Apply(op, reg, api);

            Assert.Equal(9, op.parameters.Count());
        }

        [FactWithName]
        public void SetToOdataOpsIfNoParamsAlreadyExists()
        {
            var ctrlDescriptor2 = new HttpControllerDescriptor() { ControllerType = typeof(string) };
            var mockAction = new Mock<SwaggerActionDesc>(ctrlDescriptor2);
            mockAction.Setup(c => c.GetCustomAttributes<UseOdataInSwaggerAttribute>())
                .Returns(new Collection<UseOdataInSwaggerAttribute> { new UseOdataInSwaggerAttribute() });

            api.ActionDescriptor = mockAction.Object;
            swaggerDef.Apply(op, reg, api);

            Assert.Equal(8, op.parameters.Count());
        }

        [FactWithName]
        public void UseOdataAddsSelectParameter()
        {
            api.ActionDescriptor = oDataAction;
            swaggerDef.Apply(op, reg, api);

            var odataOp = op.parameters.First(c => c.name == "$select");

            Assert.Equal("string", odataOp.type);
            Assert.Equal("query", odataOp.@in);
            Assert.Equal(false, odataOp.required);
            Assert.Equal("Select columns using OData syntax.", odataOp.description);
        }

        [FactWithName]
        public void UseOdataAddsExpandParameter()
        {
            api.ActionDescriptor = oDataAction;
            swaggerDef.Apply(op, reg, api);

            var odataOp = op.parameters.First(c => c.name == "$expand");

            Assert.Equal("string", odataOp.type);
            Assert.Equal("query", odataOp.@in);
            Assert.Equal(false, odataOp.required);
            Assert.Equal("Expand nested data using OData syntax.", odataOp.description);
        }

        [FactWithName]
        public void UseOdataAddsOrderByParameter()
        {
            api.ActionDescriptor = oDataAction;
            swaggerDef.Apply(op, reg, api);

            var odataOp = op.parameters.First(c => c.name == "$orderby");

            Assert.Equal("string", odataOp.type);
            Assert.Equal("query", odataOp.@in);
            Assert.Equal(false, odataOp.required);
            Assert.Equal("Order the results using OData syntax.", odataOp.description);
        }

        [FactWithName]
        public void UseOdataAddsSkipParameter()
        {
            api.ActionDescriptor = oDataAction;
            swaggerDef.Apply(op, reg, api);

            var odataOp = op.parameters.First(c => c.name == "$skip");

            Assert.Equal("integer", odataOp.type);
            Assert.Equal("query", odataOp.@in);
            Assert.Equal(false, odataOp.required);
            Assert.Equal("The number of results to skip.", odataOp.description);
        }

        [FactWithName]
        public void UseOdataAddsTopParameter()
        {
            api.ActionDescriptor = oDataAction;
            swaggerDef.Apply(op, reg, api);

            var odataOp = op.parameters.First(c => c.name == "$top");

            Assert.Equal("integer", odataOp.type);
            Assert.Equal("query", odataOp.@in);
            Assert.Equal(false, odataOp.required);
            Assert.Equal("The number of results to return.", odataOp.description);
        }

        [FactWithName]
        public void UseOdataAddsApplyParameter()
        {
            api.ActionDescriptor = oDataAction;
            swaggerDef.Apply(op, reg, api);

            var odataOp = op.parameters.First(c => c.name == "$apply");

            Assert.Equal("string", odataOp.type);
            Assert.Equal("query", odataOp.@in);
            Assert.Equal(false, odataOp.required);
            Assert.Equal("Return applied filter.", odataOp.description);
        }

        [FactWithName]
        public void UseOdataAddsCountParameter()
        {
            api.ActionDescriptor = oDataAction;
            swaggerDef.Apply(op, reg, api);

            var odataOp = op.parameters.First(c => c.name == "$count");

            Assert.Equal("boolean", odataOp.type);
            Assert.Equal("query", odataOp.@in);
            Assert.Equal(false, odataOp.required);
            Assert.Equal("Return the total count.", odataOp.description);
        }
    }
}
