using Microsoft.Owin;
using System.IO;

namespace AutoPocoIO.LoggingMiddleware
{
    public class ContextLogParameters
    {
        public IOwinContext Context { get; set; }
        public string StatusCode { get; set; }
        public string Exception { get; set; }
        public MemoryStream RequestBuffer { get; set; }
        public MemoryStream ResponseBuffer { get; set; }
    }
}
