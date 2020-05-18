using AutoPocoIO.Middleware;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AutoPocoIO.Owin
{
    internal sealed class OwinMiddlewareRequest : IMiddlewareRequest, IDisposable
    {
        private readonly IOwinContext _context;
        private readonly MemoryStream _bodyCopy;

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

        public string Method => _context.Request.Method;
        public string Path => _context.Request.Path.Value;
        public string PathBase => _context.Request.PathBase.Value;
        public string LocalIpAddress => _context.Request.LocalIpAddress;
        public string RemoteIpAddress => _context.Request.RemoteIpAddress;
        public string GetQuery(string key) => _context.Request.Query[key];
        public Stream Body => _bodyCopy;
        public Uri RequestUri => _context.Request.Uri;


        public (T Entity, IDictionary<string, string> ErrorMessages) GetFormValues<T>() where T : class
        {
            var form = Task.Run(async () => await _context.Request.ReadFormAsync().ConfigureAwait(false))
                            .Result
                            .ToDictionary(c => c.Key, c => c.Value);

            Dictionary<string, string> ErrorMessages = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var entity = Activator.CreateInstance<T>();
            var properties = typeof(T).GetProperties();

            //foreach (var propertyInfo in properties)
            //{
            //    form.TryGetValue(propertyInfo.Name, out string[] formValue);
            //    ErrorMessages.ValidateAndSetProperty(propertyInfo, entity, formValue);
            //}

            ////Buisness logic validation
            //if (entity is IValidatable)
            //    ((IValidatable)entity).Validate(ErrorMessages, appDbContext);

            return (entity, ErrorMessages);
        }

        public void Dispose()
        {
            _bodyCopy.Dispose();
        }
    }
}