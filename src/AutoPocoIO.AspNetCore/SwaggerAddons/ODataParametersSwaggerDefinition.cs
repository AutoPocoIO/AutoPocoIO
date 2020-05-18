using AutoPocoIO.CustomAttributes;
using Microsoft.AspNetCore.Mvc.Controllers;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;


namespace AutoPocoIO.SwaggerAddons
{
    /// <summary>
    /// Adds the supported odata parameters for IQueryable endpoints 
    /// ONLY if no parameters are defined already.
    /// </summary>
    public class ODataParametersSwaggerDefinition : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (context.MethodInfo.GetCustomAttributes(false).Any(c => c.GetType() == typeof(UseOdataInSwaggerAttribute)))
            {
                if (operation.Parameters == null)
                {
                    operation.Parameters = new List<IParameter>();
                }

                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "$filter",
                    Description = "Filter the results using OData syntax.",
                    Required = false,
                    Type = "string",
                    In = "query"
                });
                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "$select",
                    Description = "Select columns using OData syntax.",
                    Required = false,
                    Type = "string",
                    In = "query"
                });
                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "$expand",
                    Description = "Expand nested data using OData syntax.",
                    Required = false,
                    Type = "string",
                    In = "query"
                });
                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "$orderby",
                    Description = "Order the results using OData syntax.",
                    Required = false,
                    Type = "string",
                    In = "query"
                });

                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "$skip",
                    Description = "The number of results to skip.",
                    Required = false,
                    Type = "integer",
                    In = "query"
                });

                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "$top",
                    Description = "The number of results to return.",
                    Required = false,
                    Type = "integer",
                    In = "query"
                });

                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "$apply",
                    Description = "Return applied filter.",
                    Required = false,
                    Type = "string",
                    In = "query"
                });

                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "$count",
                    Description = "Return the total count.",
                    Required = false,
                    Type = "boolean",
                    In = "query"
                });
            }
        }
    }
}