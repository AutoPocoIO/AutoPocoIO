using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.Filters;

namespace AutoPocoIO.SwaggerAddons
{
    /// <summary>
    /// Gives example object for JToken parameters in swagger
    /// </summary>
    internal class JTokenExample : IExamplesProvider<JToken>
    {
        public JToken GetExamples()
        {
            JObject jObject = new JObject
            {
                ["Column1"] = "string",
                ["Column2"] = 0
            };

            return jObject;
        }
    }
}