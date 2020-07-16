using AutoPocoIO.Exceptions;
using AutoPocoIO.Owin;
using AutoPocoIO.Services;
using Microsoft.Owin;
using Swashbuckle.Swagger;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AutoPocoIO.LoggingMiddleware
{

    /// <summary>
    /// Owin middleware for logging request and responses
    /// </summary>
    public class LogRequestAndResponseMiddleware : IOwinMiddlewareWithDI
    {
        private readonly ILoggingService _loggingService;
        

        /// <summary>
        /// Initialize middleware on request with services
        /// </summary>
        /// <param name="loggingService">Request scoped logging service</param>
        public LogRequestAndResponseMiddleware(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        ///<inheritdoc/>
        public OwinMiddleware NextComponent { get; set; }

        ///<inheritdoc/>
        public async Task Invoke(IOwinContext context)
        {
            Check.NotNull(context, nameof(context));

            string exception = null;

            var stream = context.Response.Body;
            using (MemoryStream RequestBuffer = new MemoryStream())
            using (MemoryStream ResponseBuffer = new MemoryStream())
            {
                context.Request.Body.CopyTo(RequestBuffer);
                RequestBuffer.Position = 0; // rewind


                // Buffer the response
                context.Response.Body = ResponseBuffer;

                try
                {
                    await NextComponent.Invoke(context).ConfigureAwait(false);
                }
#pragma warning disable CA1031 // Catch to allow for logging to continue to process
                catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    //For set exceptions from Dashboard 
                    exception = $"Message: {ex.Message}\nInner Exception: {ex.InnerException?.Message}\nStackTrace: {ex.StackTrace}";
                    context.Response.StatusCode = 500;
                    context.Response.ReasonPhrase = "InternalServerError";
                    ResponseBuffer.Flush();
#if DEBUG
                    var bytes = Encoding.UTF8.GetBytes(exception);
#else
                    var bytes = Encoding.UTF8.GetBytes("An error has occured");
#endif
                    ResponseBuffer.Write(bytes, 0, bytes.Length);
                }

                if (_loggingService != null && _loggingService.LogCount > 0)
                {
                    ContextLogParameters logParameters = new ContextLogParameters
                    {
                        Context = context,
                        Exception = exception ?? _loggingService.Exception,
                        StatusCode = _loggingService.StatusCode,
                        RequestBuffer = RequestBuffer,
                        ResponseBuffer = ResponseBuffer
                    };

                    _loggingService.AddContextInfomation(logParameters);
                    //Discard task to log off thread
                    _ = _loggingService.LogAll();
                }

                ResponseBuffer.Seek(0, SeekOrigin.Begin);
                await ResponseBuffer.CopyToAsync(stream).ConfigureAwait(false);
            }


        }
    }
}
