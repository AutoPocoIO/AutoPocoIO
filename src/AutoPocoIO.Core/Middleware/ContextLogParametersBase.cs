using System.IO;

namespace AutoPocoIO.LoggingMiddleware
{
    public abstract class ContextLogParametersBase
    {

        public abstract string RemoteIpAddress { get; }

        public abstract string DescriptionFromStatusCode { get; }

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
