using AutoPocoIO.SwaggerAddons;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Swashbuckle.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoPocoIO.AspNet.test.Swagger
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class SchemaExampleTests
    {
        readonly SchemaRegistry reg = new SchemaRegistry(new Newtonsoft.Json.JsonSerializerSettings(),
            new Dictionary<Type, Func<Schema>>(),
            new List<ISchemaFilter>(),
            new List<IModelFilter>(),
            true,
            c => c.Name,
            true, true, true);

        [TestMethod]
        public void SkipRegisteryIfNotJToken()
        {
            var schema = new Schema();
            var type = typeof(string);

            var addExample = new AddSchemaExamples();
            addExample.Apply(schema, reg, type);

            Assert.AreEqual(0, reg.Definitions.Count());
            Assert.AreEqual(null, schema.@ref);
            Assert.AreEqual(null, schema.type);
        }

        [TestMethod]
        public void AddFirstJTokenType()
        {
            var schema = new Schema();
            var type = typeof(JToken);

            var addExample = new AddSchemaExamples();
            addExample.Apply(schema, reg, type);

            Assert.AreEqual(1, reg.Definitions.Count());
            Assert.AreEqual("string", reg.Definitions["JSON"].properties["Column1"].type);
            Assert.AreEqual("integer", reg.Definitions["JSON"].properties["Column2"].type);
            Assert.AreEqual(0, reg.Definitions["JSON"].properties["Column2"].@default);

            Assert.AreEqual("#/definitions/JSON", schema.@ref);
            Assert.AreEqual(null, schema.type);
        }

        [TestMethod]
        public void SkipSecondJTokenTypeButSetRef()
        {
            var schema = new Schema();
            var type = typeof(JToken);
            reg.Definitions.Add("JSON", new Schema());

            var addExample = new AddSchemaExamples();
            addExample.Apply(schema, reg, type);

            Assert.AreEqual(1, reg.Definitions.Count());

            Assert.AreEqual("#/definitions/JSON", schema.@ref);
            Assert.AreEqual(null, schema.type);
        }
    }
}
