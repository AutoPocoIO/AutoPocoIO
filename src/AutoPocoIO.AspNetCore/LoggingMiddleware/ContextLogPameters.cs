using Microsoft.AspNetCore.Http;
using System.IO;

namespace AutoPocoIO.LoggingMiddleware
{
    public class ContextLogParameters
    {
        public HttpContext Context { get; set; }
        public string StatusCode { get; set; }
        public string Exception { get; set; }
        public MemoryStream RequestBuffer { get; set; }
        public MemoryStream ResponseBuffer { get; set; }
    }
}
