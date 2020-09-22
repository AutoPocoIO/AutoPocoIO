using Microsoft.Owin;
using System.Globalization;

namespace AutoPocoIO.LoggingMiddleware
{
    /// <summary>
    /// 
    /// </summary>
    public class ContextLogParameters : ContextLogParametersBase
    {
        /// <summary>
        /// 
        /// </summary>
        public IOwinContext Context { get; set; }

        public override string RemoteIpAddress => Context.Request.RemoteIpAddress.ToString(CultureInfo.InvariantCulture);

        public override string DescriptionFromStatusCode
        {
            get
            {
                var response = Context.Response;
                return $"{response.StatusCode} : {(string.IsNullOrEmpty(StatusCode) ? response.ReasonPhrase : StatusCode)}";
            }
        }

    }
}
