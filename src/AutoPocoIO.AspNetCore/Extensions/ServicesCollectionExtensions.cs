using AutoPocoIO.Services;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Net.Http.Headers;
using Swashbuckle.AspNetCore.Filters;
using System.Linq;



namespace AutoPocoIO.Extensions
{
    /// <summary>
    /// Configure AspNet Core Services 
    /// </summary>
    public static partial class ServicesCollectionExtensions
    {
        /// <summary>
        /// Include Swagger and Odata services 
        /// </summary>
        /// <param name="services">The services being used to configure the context.</param>
        /// <returns>The services collection so that further configuration can be chained.</returns>
        public static IServiceCollection AddAutoPoco(this IServiceCollection services)
        {
            //Try to add if missing
            services.TryAddSingleton(new AutoPocoServiceOptions());
            services.TryAddTransient<IRequestQueryStringService, RequestQueryStringService>();

            services.AddSwaggerGen(SwaggerConfig.SwaggerServicesFunc);
            services.AddSwaggerExamplesFromAssemblyOf<SwaggerConfig>();

            //Enable OData
            services.AddOData();
            services.AddMvcCore(options =>
            {
                foreach (var outputFormatter in options.OutputFormatters.OfType<ODataOutputFormatter>().Where(_ => _.SupportedMediaTypes.Count == 0))
                {
                    outputFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/prs.odatatestxx-odata"));
                }
                foreach (var inputFormatter in options.InputFormatters.OfType<ODataInputFormatter>().Where(_ => _.SupportedMediaTypes.Count == 0))
                {
                    inputFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/prs.odatatestxx-odata"));
                }
            });

            services.AddDatabaseOperations();

            //Core only servies
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddMvcCore()
                    .AddApiExplorer();

            return services;
        }
    }
}
