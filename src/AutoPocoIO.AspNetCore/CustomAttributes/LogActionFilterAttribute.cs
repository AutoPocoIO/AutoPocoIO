using EDC.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Threading.Tasks;

namespace EDC.CustomAttributes
{
   /* public class LogActionFilterAttribute : ActionFilterAttribute
    {
        private LoggingService _loggingService;

        public LogActionFilterAttribute(LoggingService loggingService)
        {
            _loggingService = loggingService;
        }
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {

            if (_loggingService != null)
            {
                _loggingService.ResponseTime = DateTime.UtcNow;
                _loggingService.StatusCode = filterContext.HttpContext.Response.StatusCode + " : " + filterContext.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase;
                _loggingService.Ip = filterContext.HttpContext.Connection.RemoteIpAddress.ToString();

                _ = Task.Run(() => _loggingService.LogAll());
            }
        }
    }*/
}
