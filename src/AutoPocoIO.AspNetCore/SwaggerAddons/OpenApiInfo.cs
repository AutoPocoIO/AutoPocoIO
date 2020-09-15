#if NETCORE2_2

using Swashbuckle.AspNetCore.Swagger;
using System.Collections.Generic;

namespace Microsoft.OpenApi.Models
{
    /// <summary>
    /// Fill for old version of Swashbuckler in standard 2.0
    /// </summary>
    internal class OpenApiInfo : Info
    {
    }

    /// <summary>
    /// Fill for old version of Swashbuckler in standard 2.0
    /// </summary>
    public class OpenApiOperation : Operation
    {
    }

    internal class OpenApiParameter : IParameter
    {
        public string Name { get; set; }

        public ParameterLocation In { get; set; }
        string IParameter.In { get => In.ToString(); set => throw new System.NotImplementedException(); }
        public string Description { get; set; }
        public bool Required { get; set; }

        public Dictionary<string, object> Extensions => throw new System.NotImplementedException();
    }

    internal enum ParameterLocation
    {
        Query
    }


}
#endif
