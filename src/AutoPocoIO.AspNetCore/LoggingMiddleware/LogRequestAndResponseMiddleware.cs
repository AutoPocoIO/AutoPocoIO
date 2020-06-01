using AutoPocoIO.Exceptions;
using AutoPocoIO.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace AutoPocoIO.LoggingMiddleware
{
    public class LogRequestAndResponseMiddleware: IDisposable
    {
        private readonly RequestDelegate _next;

        private IServiceScopeFactory _serviceScopeFactory;
        private ILoggingService _loggingService;

        private bool isDisposed;
        private MemoryStream RequestBuffer;
        private MemoryStream ResponseBuffer;

        public LogRequestAndResponseMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public virtual async Task InvokeAsync(HttpContext context, ILoggingService loggingService)
        {
            Check.NotNull(context, nameof(context));
            Check.NotNull(loggingService, nameof(loggingService));

            _loggingService = loggingService;

            string statusCode = null;
            string exception = null;

            var stream = context.Response.Body;
            RequestBuffer = new MemoryStream();
            context.Request.Body.CopyTo(RequestBuffer);
            RequestBuffer.Position = 0; // rewind
            context.Request.Body = RequestBuffer;

            // Buffer the response
            ResponseBuffer = new MemoryStream();
            context.Response.Body = ResponseBuffer;


            try
            {
                await _next.Invoke(context).ConfigureAwait(false);
            }
            catch (BaseCaughtException ex)
            {
                statusCode = ex.HttpErrorMessage;
                exception = ex.Message;
                context.Response.StatusCode = (int)ex.ResponseCode;
                context.Response.ContentType = MediaTypeNames.Text.Plain;
                ResponseBuffer = new MemoryStream(Encoding.UTF8.GetBytes(ex.Message));
            }
#pragma warning disable CA1031 // Catch to allow for logging to continue to process
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {

                context.Response.StatusCode = 500;

                exception = $"Message: {ex.Message}\nInner Exception: {ex.InnerException?.Message}\nStackTrace: {ex.StackTrace}";
                context.Response.StatusCode = 500;
                context.Response.ContentType = MediaTypeNames.Text.Plain;

#if DEBUG
                ResponseBuffer = new MemoryStream(Encoding.UTF8.GetBytes(exception));
#else
                ResponseBuffer = new MemoryStream(Encoding.UTF8.GetBytes("An error has occured"));
#endif
            }

            if (_loggingService.LogCount > 0)
            {
                ContextLogParameters logParameters = new ContextLogParameters
                {
                    Context = context,
                    StatusCode = statusCode,
                    Exception = exception,
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed) return;
            if (disposing)
            {
                RequestBuffer.Dispose();
                ResponseBuffer.Dispose();
            }

            isDisposed = true;
        }

    }
}
