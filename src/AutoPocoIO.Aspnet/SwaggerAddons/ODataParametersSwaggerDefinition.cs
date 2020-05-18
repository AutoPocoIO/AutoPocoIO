using AutoPocoIO.CustomAttributes;
using Swashbuckle.Swagger;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Description;


namespace AutoPocoIO.SwaggerAddons
{
    /// <summary>
    /// Adds the supported odata parameters for IQueryable endpoints 
    /// ONLY if no parameters are defined already.
    /// </summary>
    public class ODataParametersSwaggerDefinition : IOperationFilter
    {
        /// <summary>
        /// Apply the filter to the operation.
        /// </summary>
        /// <param name="operation">The API operation to check.</param>
        /// <param name="schemaRegistry">The swagger schema registry.</param>
        /// <param name="apiDescription">The description of the api method.</param>
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            var responseType = apiDescription.ResponseType();
            if (responseType != null)
            {
                if (apiDescription.ActionDescriptor.GetCustomAttributes<UseOdataInSwaggerAttribute>().Any())
                {
                    if (operation.parameters == null)
                    {
                        operation.parameters = new List<Parameter>();
                    }

                    operation.parameters.Add(new Parameter
                    {
                        name = "$filter",
                        description = "Filter the results using OData syntax.",
                        required = false,
                        type = "string",
                        @in = "query"
                    });
                    operation.parameters.Add(new Parameter
                    {
                        name = "$select",
                        description = "Select columns using OData syntax.",
                        required = false,
                        type = "string",
                        @in = "query"
                    });
                    operation.parameters.Add(new Parameter
                    {
                        name = "$expand",
                        description = "Expand nested data using OData syntax.",
                        required = false,
                        type = "string",
                        @in = "query"
                    });
                    operation.parameters.Add(new Parameter
                    {
                        name = "$orderby",
                        description = "Order the results using OData syntax.",
                        required = false,
                        type = "string",
                        @in = "query"
                    });

                    operation.parameters.Add(new Parameter
                    {
                        name = "$skip",
                        description = "The number of results to skip.",
                        required = false,
                        type = "integer",
                        @in = "query"
                    });

                    operation.parameters.Add(new Parameter
                    {
                        name = "$top",
                        description = "The number of results to return.",
                        required = false,
                        type = "integer",
                        @in = "query"
                    });

                    operation.parameters.Add(new Parameter
                    {
                        name = "$apply",
                        description = "Return applied filter.",
                        required = false,
                        type = "string",
                        @in = "query"
                    });

                    operation.parameters.Add(new Parameter
                    {
                        name = "$count",
                        description = "Return the total count.",
                        required = false,
                        type = "boolean",
                        @in = "query"
                    });
                }
            }
        }
    }
}