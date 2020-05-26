using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AutoPocoIO.Middleware
{
    internal sealed partial class AspNetCoreMiddlewareRequest : IMiddlewareRequest
    {
        private readonly HttpContext _context;
        public AspNetCoreMiddlewareRequest(HttpContext context)
        {
            _context = context;
        }

        public string Method => _context.Request.Method;
        public string Path => _context.Request.Path.Value;
        public string PathBase => _context.Request.PathBase.Value;
        public string LocalIpAddress => _context.Connection.LocalIpAddress.ToString();
        public string RemoteIpAddress => _context.Connection.RemoteIpAddress.ToString();
        public string GetQuery(string key) => _context.Request.Query[key];
        public Uri RequestUri => new Uri(_context.Request.GetEncodedUrl());
        public Stream Body => _context.Request.Body;


        public IEnumerable<KeyValuePair<string, string>> Cookies => _context.Request.Cookies;

        public (T Entity, IDictionary<string, string> ErrorMessages) GetFormValues<T>() where T: class
        {
            Dictionary<string, StringValues> form;
            try
            {
                form = _context.Request.ReadFormAsync()
                              .Result
                              .ToDictionary(c => c.Key, c => c.Value);
            }
            catch (InvalidOperationException)
            {
                form = new Dictionary<string, StringValues>();
            }

            Dictionary<string, string> ErrorMessages = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var entity = Activator.CreateInstance<T>();

            var properties = typeof(T).GetProperties();

            //foreach (var propertyInfo in properties)
            //{
            //    if (propertyInfo != null)
            //    {
            //        form.TryGetValue(propertyInfo.Name, out StringValues formValue);
            //        ErrorMessages.ValidateAndSetProperty(propertyInfo, entity, formValue);
            //    }
            //}

            ////Buisness logic validation
            //if (entity is IValidatable)
            //    ((IValidatable)entity).Validate(ErrorMessages, _appDb);

            return (entity, ErrorMessages);
        }

        public async Task<IDictionary<string, string[]>> ReadFormAsync()
        {
            var form = await _context.Request.ReadFormAsync().ConfigureAwait(false);
            return form.ToDictionary(c => c.Key, c => c.Value.ToArray());
        }
    }
}