using Swashbuckle.Application;
using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Routing;
using System.Xml;
using System.Xml.XPath;

namespace AutoPocoIO.SwaggerAddons
{
    internal static class SwaggerConfig
    {
        public static void Register(HttpConfiguration configuration, string dashPath)
        {
            configuration
                .EnableSwagger(dashPath.Trim('/') + "/swagger/docs/{apiVersion}", SwaggerDocsFunc)
                .EnableSwaggerUi(dashPath.Trim('/') + "/swagger/ui/{*assetPath}", SwaggerUiFunc);

            configuration.Routes.MapHttpRoute(
                   name: "autopoco_swagger_shortcut",
                   routeTemplate: dashPath.Trim('/') + "/swagger",
                   defaults: null,
                   constraints: new { uriResolution = new HttpRouteDirectionConstraint(HttpRouteDirection.UriResolution) },
                   handler: new RedirectHandler(c => c.RequestUri.Scheme + "://" + c.RequestUri.Authority + dashPath, "swagger/ui/index"));
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

        public static Action<SwaggerDocsConfig> SwaggerDocsFunc
        {
            get
            {
                Assembly thisAssembly = typeof(SwaggerConfig).Assembly;
                return c =>
                {
                    c.RootUrl(req => req.RequestUri.GetLeftPart(System.UriPartial.Authority)
                                  + req.GetRequestContext().VirtualPathRoot.TrimEnd('/'));

                    c.SingleApiVersion("v1", "AutoPoco");
                    c.OperationFilter<ODataParametersSwaggerDefinition>();

                    c.SchemaFilter<AddSchemaExamples>();

                    c.GroupActionsBy(apiDesc =>
                    {
                        return System.Text.RegularExpressions.Regex.Replace(apiDesc.ActionDescriptor.ControllerDescriptor.ControllerName, @"((?<=[A-Z])([A-Z])(?=[a-z]))|((?<=[a-z]+)([A-Z]))", @" $0", System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
                    });

                    c.IncludeXmlComments(() => GetResource(thisAssembly));
                };
            }
        }



        private static Action<SwaggerUiConfig> SwaggerUiFunc
        {
            get
            {
                Assembly thisAssembly = typeof(SwaggerConfig).Assembly;
                return c =>
                {
                    c.DocumentTitle("AutoPoco");
                    c.InjectStylesheet(thisAssembly, $"{thisAssembly.GetName().Name}.SwaggerAddons.swagger.css");
                    c.EnableApiKeySupport("Authorization", "header");
                };
            }
        }
    }
}
