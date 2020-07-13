using AutoPocoIO.Extensions;
using System.Collections.Generic;
using System.Web;

namespace AutoPocoIO.Services
{
    /// <summary>
    /// Access Http Request query strings.
    /// </summary>
    public class RequestQueryStringService : IRequestQueryStringService
    {
        ///<inheritdoc/>
        public IDictionary<string, string> GetQueryStrings()
        {
            return HttpContext.Current.Request.GetQueryStrings();
        }
    }
}
