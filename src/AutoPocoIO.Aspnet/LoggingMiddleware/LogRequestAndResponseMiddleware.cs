using AutoPocoIO.Exceptions;
using AutoPocoIO.Owin;
using AutoPocoIO.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AutoPocoIO.LoggingMiddleware
{

    public class LogRequestAndResponseMiddleware : IOwinMiddlewareWithDI, IDisposable
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILoggingService _loggingService;
        
        private bool isDisposed;
        private MemoryStream RequestBuffer;
        private MemoryStream ResponseBuffer;


        public LogRequestAndResponseMiddleware(
            IServiceScopeFactory providerScope,
            ILoggingService loggingService)
        {
            _serviceScopeFactory = providerScope;
            _loggingService = loggingService;
        }

        public OwinMiddleware Next { get; set; }

        public async Task Invoke(IOwinContext context)
        {
            Check.NotNull(context, nameof(context));

            string exception = null;

            var stream = context.Response.Body;
            RequestBuffer = new MemoryStream();
            context.Request.Body.CopyTo(RequestBuffer);
            RequestBuffer.Position = 0; // rewind


            // Buffer the response
            ResponseBuffer = new MemoryStream();
            context.Response.Body = ResponseBuffer;

            try
            {
                await Next.Invoke(context).ConfigureAwait(false);
            }
#pragma warning disable CA1031 // Catch to allow for logging to continue to process
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                //For set exceptions from Dashboard 
                exception = $"Message: {ex.Message}\nInner Exception: {ex.InnerException?.Message}\nStackTrace: {ex.StackTrace}";
                context.Response.StatusCode = 500;
                context.Response.ReasonPhrase = "InternalServerError";

#if DEBUG
                ResponseBuffer = new MemoryStream(Encoding.UTF8.GetBytes(exception));
#else
                ResponseBuffer = new MemoryStream(Encoding.UTF8.GetBytes("An error has occured"));
#endif

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

                //create thread scope
                var scope = _serviceScopeFactory.CreateScope();
                _ = Task.Run(() =>
                {
                    try
                    {
                        _loggingService.LogAll(scope);
                    }
                    finally
                    {
                        scope.Dispose();
                    }
                });
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
            if(disposing)
            {
                RequestBuffer.Dispose();
                ResponseBuffer.Dispose();
            }

            isDisposed = true;
        }
    }
}
