using AutoPocoIO.Extensions;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace AutoPocoIO.Services
{
    /// <summary>
    /// Access Http Request query strings.
    /// </summary>
    public class RequestQueryStringService : IRequestQueryStringService
    {
        private readonly IHttpContextAccessor _contextAccessor;

        /// <summary>
        /// Initialize with access to <see cref="HttpContext"/>
        /// </summary>
        /// <param name="contextAccessor"></param>
        public RequestQueryStringService(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        ///<inheritdoc/>
        public IDictionary<string, string> GetQueryStrings()
        {
            return _contextAccessor.HttpContext.Request.GetQueryStrings();
        }
    }
}
