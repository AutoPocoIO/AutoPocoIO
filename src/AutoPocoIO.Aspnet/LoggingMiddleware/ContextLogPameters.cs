using Microsoft.Owin;
using System.IO;

namespace AutoPocoIO.LoggingMiddleware
{
    /// <summary>
    /// 
    /// </summary>
    public class ContextLogParameters
    {
        /// <summary>
        /// 
        /// </summary>
        public IOwinContext Context { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string StatusCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Exception { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public MemoryStream RequestBuffer { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public MemoryStream ResponseBuffer { get; set; }
    }
}
