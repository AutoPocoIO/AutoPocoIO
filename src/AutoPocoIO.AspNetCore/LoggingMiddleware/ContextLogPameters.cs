using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

namespace AutoPocoIO.LoggingMiddleware
{
    /// <summary>
    /// 
    /// </summary>
    public class ContextLogParameters : ContextLogParametersBase
    {
        /// <summary>
        ///  Gets or sets the <see cref="HttpResponse.HttpContext"/> for this request.
        /// </summary>
        public HttpContext Context { get; set; }

        public override string RemoteIpAddress => Context.Connection.RemoteIpAddress.ToString();

        public override string DescriptionFromStatusCode
        {
            get
            {
                var response = Context.Response;
                return $"{response.StatusCode} : {(string.IsNullOrEmpty(StatusCode) ? ReasonPhrases.GetReasonPhrase(response.StatusCode) : StatusCode)}";
            }
        }
    }
}
