using AutoPocoIO.Exceptions;
using Newtonsoft.Json.Linq;
using Swashbuckle.Swagger;
using System;
using System.Collections.Generic;

namespace AutoPocoIO.SwaggerAddons
{
    public class AddSchemaExamples : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            Check.NotNull(schema, nameof(schema));
            Check.NotNull(schemaRegistry, nameof(schemaRegistry));
            Check.NotNull(type, nameof(type));

            if (type == typeof(JToken))
            {
                if (!schemaRegistry.Definitions.ContainsKey("JSON"))
                {
                    Schema jtokenSchema = new Schema
                    {
                        properties = new Dictionary<string, Schema>
                        {
                            {"Column1", new Schema() { type = "string" } },
                            {"Column2", new Schema() { type = "integer", @default = 0 } }
                        }
                    };
                    schemaRegistry.Definitions.Add("JSON", jtokenSchema);
                }

                schema.@ref = "#/definitions/JSON";
                schema.@type = null;
            }

        }
    }
}