using AutoPocoIO.SwaggerAddons;
using Xunit;
using Newtonsoft.Json.Linq;
using Swashbuckle.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoPocoIO.AspNet.test.Swagger
{
    
    [Trait("Category", TestCategories.Unit)]
    public class SchemaExampleTests
    {
        readonly SchemaRegistry reg = new SchemaRegistry(new Newtonsoft.Json.JsonSerializerSettings(),
            new Dictionary<Type, Func<Schema>>(),
            new List<ISchemaFilter>(),
            new List<IModelFilter>(),
            true,
            c => c.Name,
            true, true, true);

        [FactWithName]
        public void SkipRegisteryIfNotJToken()
        {
            var schema = new Schema();
            var type = typeof(string);

            var addExample = new AddSchemaExamples();
            addExample.Apply(schema, reg, type);

            Assert.Empty(reg.Definitions);
            Assert.Null(schema.@ref);
            Assert.Null(schema.type);
        }

        [FactWithName]
        public void AddFirstJTokenType()
        {
            var schema = new Schema();
            var type = typeof(JToken);

            var addExample = new AddSchemaExamples();
            addExample.Apply(schema, reg, type);

            Assert.Single(reg.Definitions);
            Assert.Equal("string", reg.Definitions["JSON"].properties["Column1"].type);
            Assert.Equal("integer", reg.Definitions["JSON"].properties["Column2"].type);
            Assert.Equal(0, reg.Definitions["JSON"].properties["Column2"].@default);

            Assert.Equal("#/definitions/JSON", schema.@ref);
            Assert.Null(schema.type);
        }

        [FactWithName]
        public void SkipSecondJTokenTypeButSetRef()
        {
            var schema = new Schema();
            var type = typeof(JToken);
            reg.Definitions.Add("JSON", new Schema());

            var addExample = new AddSchemaExamples();
            addExample.Apply(schema, reg, type);

            Assert.Single(reg.Definitions);

            Assert.Equal("#/definitions/JSON", schema.@ref);
            Assert.Null(schema.type);
        }
    }
}
