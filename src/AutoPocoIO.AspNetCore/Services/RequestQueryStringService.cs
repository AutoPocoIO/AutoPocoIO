using AutoPocoIO.Extensions;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace AutoPocoIO.Services
{
    public class RequestQueryStringService : IRequestQueryStringService
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public RequestQueryStringService(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public IDictionary<string, string> GetQueryStrings()
        {
            return _contextAccessor.HttpContext.Request.GetQueryStrings();
        }
    }
}
