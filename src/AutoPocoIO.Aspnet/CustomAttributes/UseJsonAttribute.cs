using AutoPocoIO.Exceptions;
using AutoPocoIO.Services;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Filters;

namespace AutoPocoIO.CustomAttributes
{
    public class UseJsonAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext == null) throw new ArgumentNullException(nameof(actionExecutedContext));

            if (actionExecutedContext.Response != null)
            {
                ObjectContent content = actionExecutedContext.Response.Content as ObjectContent;
                var value = content.Value;
                Type targetType = actionExecutedContext.Response.Content.GetType().GetGenericArguments()[0];

                var jsonFormater = new JsonMediaTypeFormatter();
                jsonFormater.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

                var httpResponseMsg = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    RequestMessage = actionExecutedContext.Request,
                    Content = new ObjectContent(targetType, value, jsonFormater, (string)null)
                };

                actionExecutedContext.Response = httpResponseMsg;
            }

            var scope = actionExecutedContext.Request.GetDependencyScope();


            if (actionExecutedContext.Exception != null && scope.GetService(typeof(ILoggingService)) is ILoggingService loggingService)
            {
                loggingService.Exception = $"Exception: {actionExecutedContext.Exception.Message}\nInner Exception: {actionExecutedContext.Exception.InnerException?.Message}\nStack Trace: {actionExecutedContext.Exception.StackTrace}";
                if (actionExecutedContext.Exception is BaseCaughtException ex)
                {
                    var httpResponseMsg = new HttpResponseMessage
                    {
                        StatusCode = ex.ResponseCode,
                        RequestMessage = actionExecutedContext.Request,
                    };

#if DEBUG
                    httpResponseMsg.Content = new StringContent(loggingService.Exception);
#else
                   httpResponseMsg.Content = new StringContent($"Message: {ex.Message}");
#endif
                    actionExecutedContext.Response = httpResponseMsg;

                    loggingService.StatusCode = $"{(int)ex.ResponseCode} : {ex.HttpErrorMessage}";

                }
            }
            base.OnActionExecuted(actionExecutedContext);
        }
    }
}