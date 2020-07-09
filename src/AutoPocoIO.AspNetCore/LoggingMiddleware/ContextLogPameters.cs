using Microsoft.AspNetCore.Http;
using System.IO;

namespace AutoPocoIO.LoggingMiddleware
{
    /// <summary>
    /// 
    /// </summary>
    public class ContextLogParameters
    {
        /// <summary>
        ///  Gets or sets the <see cref="HttpResponse.HttpContext"/> for this request.
        /// </summary>
        public HttpContext Context { get; set; }
        /// <summary>
        /// Gets or sets the HTTP response code.
        /// </summary>
        public string StatusCode { get; set; }
        /// <summary>
        /// Request exception to log
        /// </summary>
        public string Exception { get; set; }
        /// <summary>
        /// Buffered Request body stream
        /// </summary>
        public MemoryStream RequestBuffer { get; set; }
        /// <summary>
        /// Buffered Response body stream
        /// </summary>
        public MemoryStream ResponseBuffer { get; set; }
    }
}
