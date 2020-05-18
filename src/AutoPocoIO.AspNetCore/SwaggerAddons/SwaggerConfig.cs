using AutoPocoIO.SwaggerAddons;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;

namespace AutoPocoIO
{
    internal class SwaggerConfig
    {

        public static Action<SwaggerGenOptions> SwaggerServicesFunc
        {
            get
            {
                Assembly thisAssembly = typeof(SwaggerConfig).Assembly;

                return c =>
                {
                    c.SwaggerDoc("v1", new Info { Title = "AutoPoco", Version = "v1" });
                    c.OperationFilter<ODataParametersSwaggerDefinition>();
                    c.ExampleFilters();

                    c.TagActionsBy(apiDesc =>
                    {
                        if (apiDesc.ActionDescriptor is ControllerActionDescriptor actionDescriptor)
                            return new string[]{
                                System.Text.RegularExpressions.Regex.Replace(actionDescriptor.ControllerName, @"((?<=[A-Z])([A-Z])(?=[a-z]))|((?<=[a-z]+)([A-Z]))", @" $0", System.Text.RegularExpressions.RegexOptions.Compiled).Trim()
                            };
                        else
                            return Array.Empty<string>();
                    });

                    c.IncludeXmlComments(() => GetResource(thisAssembly));
                };
            }
        }

        public static Action<SwaggerOptions> SwaggerAppBuilderFunc(string dashPath)
        {
            return c =>
            {
                c.RouteTemplate = dashPath.Trim('/') + "/swagger/{documentname}/swagger.json";
            };
        }

        public static Action<SwaggerUIOptions> SwaggerUIAppBuilderFunc(string dashPath)
        {
            return c =>
            {
                c.DocumentTitle = "AutoPoco";
                c.RoutePrefix = dashPath.Trim('/') + "/swagger";
                c.SwaggerEndpoint("v1/swagger.json", "AutoPoco v1");
                c.DocExpansion(DocExpansion.None);
            };
        }

        private static XPathDocument GetResource(Assembly assembly)
        {
            Stream s = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.SwaggerAddons.AutoPoco.xml");
            using (XmlReader xr = XmlReader.Create(s, new XmlReaderSettings() { DtdProcessing = DtdProcessing.Prohibit }))
            {
                XPathDocument xdoc = new XPathDocument(xr);
                return xdoc;
            }
        }
    }
}
