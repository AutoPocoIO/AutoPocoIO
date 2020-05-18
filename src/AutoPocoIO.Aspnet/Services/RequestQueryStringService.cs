using AutoPocoIO.Extensions;
using System.Collections.Generic;
using System.Web;

namespace AutoPocoIO.Services
{
    public class RequestQueryStringService : IRequestQueryStringService
    {
        public IDictionary<string, string> GetQueryStrings()
        {
            return HttpContext.Current.Request.GetQueryStrings();
        }
    }
}
