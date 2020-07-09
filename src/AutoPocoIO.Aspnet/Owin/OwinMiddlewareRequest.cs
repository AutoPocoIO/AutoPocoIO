using AutoPocoIO.Middleware;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AutoPocoIO.Owin
{
    /// <summary>
    /// Represents the incoming side of an individual HTTP request.
    /// </summary>
    public class OwinMiddlewareRequest : IMiddlewareRequest, IDisposable
    {
        private readonly IOwinContext _context;
        private readonly MemoryStream _bodyCopy;
        private bool _isDisposed;

        /// <summary>
        /// Initilize the middleware request from the <paramref name="environment"/>
        /// </summary>
        /// <param name="environment">Owin environment to set properties</param>
        public OwinMiddlewareRequest(IDictionary<string, object> environment)
        {

            _context = new OwinContext(environment);
            _bodyCopy = new MemoryStream();

            if (_context.Request.Body?.Length > 0)
            {

                _context.Request.Body.CopyTo(_bodyCopy);
                MemoryStream replaceBody = new MemoryStream();

                _bodyCopy.Position = 0;
                _bodyCopy.CopyTo(replaceBody);

                _bodyCopy.Position = 0;
                replaceBody.Position = 0;

                _context.Request.Body = replaceBody;
            }
        }

        ///<inheritdoc/>
        public string Method => _context.Request.Method;
        ///<inheritdoc/>
        public string Path => _context.Request.Path.Value;
        ///<inheritdoc/>
        public string PathBase => _context.Request.PathBase.Value;
        ///<inheritdoc/>
        public string LocalIpAddress => _context.Request.LocalIpAddress;
        ///<inheritdoc/>
        public string RemoteIpAddress => _context.Request.RemoteIpAddress;
        ///<inheritdoc/>
        public string GetQuery(string key) => _context.Request.Query[key];
        ///<inheritdoc/>
        public Stream Body => _bodyCopy;
        ///<inheritdoc/>
        public Uri RequestUri => _context.Request.Uri;
        ///<inheritdoc/>
        public async Task<IDictionary<string, string[]>> ReadFormAsync()
        {
            return (await _context.Request.ReadFormAsync().ConfigureAwait(false))
                            .ToDictionary(c => c.Key, c => c.Value);
        }

        ///<inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ///<inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                _bodyCopy.Dispose();
            }

        

            _isDisposed = true;
        }
    }
}