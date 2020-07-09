using AutoPocoIO.Middleware;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AutoPocoIO.Owin
{
    /// <summary>
    /// Represents the outgoing side of an individual HTTP request.
    /// </summary>
    public class OwinMiddlewareResponse : IMiddlewareResponse
    {
        private readonly IOwinContext _context;

        /// <summary>
        /// Initilize the middleware response from the <paramref name="environment"/>
        /// </summary>
        /// <param name="environment">Owin environment to set properties</param>
        public OwinMiddlewareResponse(IDictionary<string, object> environment)
        {
            _context = new OwinContext(environment);
        }

        ///<inheritdoc/>
        public string ContentType
        {
            get { return _context.Response.ContentType; }
            set { _context.Response.ContentType = value; }
        }
        ///<inheritdoc/>
        public int StatusCode
        {
            get { return _context.Response.StatusCode; }
            set { _context.Response.StatusCode = value; }
        }
        ///<inheritdoc/>
        public Stream Body => _context.Response.Body;
        ///<inheritdoc/>
        public void Redirect(string location)
        {
            _context.Response.Redirect(location);
        }
        ///<inheritdoc/>
        public void SetExpire(DateTimeOffset? value)
        {
            _context.Response.Expires = value;
        }
        ///<inheritdoc/>
        public Task WriteAsync(string text)
        {
            return _context.Response.WriteAsync(text);
        }
    }
}
