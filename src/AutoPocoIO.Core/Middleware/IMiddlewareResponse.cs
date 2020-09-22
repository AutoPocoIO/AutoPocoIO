using System;
using System.IO;
using System.Threading.Tasks;

namespace AutoPocoIO.Middleware
{
    /// <summary>
    /// Represents the outgoing side of an individual HTTP request.
    /// </summary>
    public interface IMiddlewareResponse
    {
        /// <summary>
        /// Gets or sets the value for the Content-Type response header.
        /// </summary>
        string ContentType { get; set; }
        /// <summary>
        /// Gets or sets the HTTP response code.
        /// </summary>
        int StatusCode { get; set; }
        /// <summary>
        /// Gets or sets the response body <see cref="Stream"/>.
        /// </summary>
        Stream Body { get; }
        /// <summary>
        /// Set <c>"Expires"</c> key in header dictionary
        /// </summary>
        /// <param name="value"></param>
        void SetExpire(DateTimeOffset? value);
        /// <summary>
        ///  Writes the given text to the response body. UTF-8 encoding will be used.
        /// </summary>
        /// <param name="text">The text to write to the response.</param>
        /// <returns>A task that represents the completion of the write operation.</returns>
        Task WriteAsync(string text);
        /// <summary>
        ///  Returns a temporary redirect response (HTTP 302) to the client.
        /// </summary>
        /// <param name="location"> The URL to redirect the client to. This must be properly encoded for use in http
        ///     headers where only ASCII characters are allowed.</param>
        void Redirect(string location);
    }
}