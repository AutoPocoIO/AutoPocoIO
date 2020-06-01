using AutoPocoIO.CustomAttributes;
using AutoPocoIO.SwaggerAddons;
using AutoPocoIO.WebApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    [TestClass]
    [TestCategory(TestCategories.Unit)]
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

        [TestInitialize]
        public void Init()
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


        [TestMethod]
        public void DoNothingIfResponseTypeIsNull()
        {
            api.ResponseDescription.ResponseType = null;
            swaggerDef.Apply(op, reg, api);

            Assert.IsNull(op.parameters);
        }

        [TestMethod]
        public void UseOdataAddsFilterParameter()
        {
            api.ActionDescriptor = oDataAction;
            swaggerDef.Apply(op, reg, api);

            var odataOp = op.parameters.First(c => c.name == "$filter");

            Assert.AreEqual("string", odataOp.type);
            Assert.AreEqual("query", odataOp.@in);
            Assert.AreEqual(false, odataOp.required);
            Assert.AreEqual("Filter the results using OData syntax.", odataOp.description);
        }

        [TestMethod]
        public void SkipOdataParamsIfMissingAttr()
        {
            op = new Operation { parameters = new List<Parameter> { new Parameter { name = "orginial" } } };
            var ctrlDescriptor2 = new HttpControllerDescriptor() { ControllerType = typeof(string) };
            var mockAction = new Mock<SwaggerActionDesc>(ctrlDescriptor2);
            mockAction.Setup(c => c.GetCustomAttributes<UseOdataInSwaggerAttribute>())
                .Returns(new Collection<UseOdataInSwaggerAttribute> { });

            api.ActionDescriptor = mockAction.Object;
            swaggerDef.Apply(op, reg, api);

            Assert.AreEqual(1, op.parameters.Count());
        }

        [TestMethod]
        public void AppendOdataOpsIfSomeAlreadyExists()
        {
            op = new Operation { parameters = new List<Parameter> { new Parameter { name = "orginial" } } };
            var ctrlDescriptor2 = new HttpControllerDescriptor() { ControllerType = typeof(string) };
            var mockAction = new Mock<SwaggerActionDesc>(ctrlDescriptor2);
            mockAction.Setup(c => c.GetCustomAttributes<UseOdataInSwaggerAttribute>())
                .Returns(new Collection<UseOdataInSwaggerAttribute> { new UseOdataInSwaggerAttribute() });

            api.ActionDescriptor = mockAction.Object;
            swaggerDef.Apply(op, reg, api);

            Assert.AreEqual(9, op.parameters.Count());
        }

        [TestMethod]
        public void SetToOdataOpsIfNoParamsAlreadyExists()
        {
            var ctrlDescriptor2 = new HttpControllerDescriptor() { ControllerType = typeof(string) };
            var mockAction = new Mock<SwaggerActionDesc>(ctrlDescriptor2);
            mockAction.Setup(c => c.GetCustomAttributes<UseOdataInSwaggerAttribute>())
                .Returns(new Collection<UseOdataInSwaggerAttribute> { new UseOdataInSwaggerAttribute() });

            api.ActionDescriptor = mockAction.Object;
            swaggerDef.Apply(op, reg, api);

            Assert.AreEqual(8, op.parameters.Count());
        }

        [TestMethod]
        public void UseOdataAddsSelectParameter()
        {
            api.ActionDescriptor = oDataAction;
            swaggerDef.Apply(op, reg, api);

            var odataOp = op.parameters.First(c => c.name == "$select");

            Assert.AreEqual("string", odataOp.type);
            Assert.AreEqual("query", odataOp.@in);
            Assert.AreEqual(false, odataOp.required);
            Assert.AreEqual("Select columns using OData syntax.", odataOp.description);
        }

        [TestMethod]
        public void UseOdataAddsExpandParameter()
        {
            api.ActionDescriptor = oDataAction;
            swaggerDef.Apply(op, reg, api);

            var odataOp = op.parameters.First(c => c.name == "$expand");

            Assert.AreEqual("string", odataOp.type);
            Assert.AreEqual("query", odataOp.@in);
            Assert.AreEqual(false, odataOp.required);
            Assert.AreEqual("Expand nested data using OData syntax.", odataOp.description);
        }

        [TestMethod]
        public void UseOdataAddsOrderByParameter()
        {
            api.ActionDescriptor = oDataAction;
            swaggerDef.Apply(op, reg, api);

            var odataOp = op.parameters.First(c => c.name == "$orderby");

            Assert.AreEqual("string", odataOp.type);
            Assert.AreEqual("query", odataOp.@in);
            Assert.AreEqual(false, odataOp.required);
            Assert.AreEqual("Order the results using OData syntax.", odataOp.description);
        }

        [TestMethod]
        public void UseOdataAddsSkipParameter()
        {
            api.ActionDescriptor = oDataAction;
            swaggerDef.Apply(op, reg, api);

            var odataOp = op.parameters.First(c => c.name == "$skip");

            Assert.AreEqual("integer", odataOp.type);
            Assert.AreEqual("query", odataOp.@in);
            Assert.AreEqual(false, odataOp.required);
            Assert.AreEqual("The number of results to skip.", odataOp.description);
        }

        [TestMethod]
        public void UseOdataAddsTopParameter()
        {
            api.ActionDescriptor = oDataAction;
            swaggerDef.Apply(op, reg, api);

            var odataOp = op.parameters.First(c => c.name == "$top");

            Assert.AreEqual("integer", odataOp.type);
            Assert.AreEqual("query", odataOp.@in);
            Assert.AreEqual(false, odataOp.required);
            Assert.AreEqual("The number of results to return.", odataOp.description);
        }

        [TestMethod]
        public void UseOdataAddsApplyParameter()
        {
            api.ActionDescriptor = oDataAction;
            swaggerDef.Apply(op, reg, api);

            var odataOp = op.parameters.First(c => c.name == "$apply");

            Assert.AreEqual("string", odataOp.type);
            Assert.AreEqual("query", odataOp.@in);
            Assert.AreEqual(false, odataOp.required);
            Assert.AreEqual("Return applied filter.", odataOp.description);
        }

        [TestMethod]
        public void UseOdataAddsCountParameter()
        {
            api.ActionDescriptor = oDataAction;
            swaggerDef.Apply(op, reg, api);

            var odataOp = op.parameters.First(c => c.name == "$count");

            Assert.AreEqual("boolean", odataOp.type);
            Assert.AreEqual("query", odataOp.@in);
            Assert.AreEqual(false, odataOp.required);
            Assert.AreEqual("Return the total count.", odataOp.description);
        }
    }
}
