using AutoPocoIO.Exceptions;
using Newtonsoft.Json.Linq;
using Swashbuckle.Swagger;
using System;
using System.Collections.Generic;

namespace AutoPocoIO.SwaggerAddons
{
    /// <summary>
    /// Gives example object for JToken parameters in swagger
    /// </summary>
    public class AddSchemaExamples : ISchemaFilter
    {
        /// <summary>
        /// Add <see cref="JToken"/> example
        /// </summary>
        /// <param name="schema">Schema to map definition.</param>
        /// <param name="schemaRegistry">List of registered schemas.</param>
        /// <param name="type">Current value to check.</param>
        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            if (schema == null) throw new ArgumentNullException(nameof(schema));
            if (schemaRegistry == null) throw new ArgumentNullException(nameof(schemaRegistry));
            if (type == null) throw new ArgumentNullException(nameof(type));

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