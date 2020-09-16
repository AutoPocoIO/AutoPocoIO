using AutoPocoIO.CustomAttributes;
using AutoPocoIO.Exceptions;
using Microsoft.OpenApi.Models;
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

        /// <summary>
        /// Add parameters for Odata operations.
        /// </summary>
        /// <param name="operation">Swagger operation</param>
        /// <param name="context">Action context</param>
#if EF22
        public void Apply(Operation operation, OperationFilterContext context)
#else
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
#endif
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(context, nameof(context));

            if (context.MethodInfo.GetCustomAttributes(false).Any(c => c.GetType() == typeof(UseOdataInSwaggerAttribute)))
            {
                if (operation.Parameters == null)
                {
#if EF22

                    operation.Parameters = new List<IParameter>();
#else
       
                    operation.Parameters = new List<OpenApiParameter>();
#endif
                }

                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "$filter",
                    Description = "Filter the results using OData syntax.",
                    Required = false,
                    In = ParameterLocation.Query
                });
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "$select",
                    Description = "Select columns using OData syntax.",
                    Required = false,
                    In = ParameterLocation.Query
                });
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "$expand",
                    Description = "Expand nested data using OData syntax.",
                    Required = false,
                    In = ParameterLocation.Query
                });
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "$orderby",
                    Description = "Order the results using OData syntax.",
                    Required = false,
                    In = ParameterLocation.Query
                });

                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "$skip",
                    Description = "The number of results to skip.",
                    Required = false,
                    In = ParameterLocation.Query
                });

                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "$top",
                    Description = "The number of results to return.",
                    Required = false,
                    In = ParameterLocation.Query
                });

                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "$apply",
                    Description = "Return applied filter.",
                    Required = false,
                    In = ParameterLocation.Query
                });

                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "$count",
                    Description = "Return the total count.",
                    Required = false,
                    In = ParameterLocation.Query
                });
            }
        }
    }
}