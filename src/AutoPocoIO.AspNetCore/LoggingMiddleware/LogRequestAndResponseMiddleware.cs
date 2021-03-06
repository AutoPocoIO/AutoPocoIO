﻿using AutoPocoIO.Exceptions;
using AutoPocoIO.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace AutoPocoIO.LoggingMiddleware
{
    /// <summary>
    /// ASP.NET Core middleware for logging request and responses
    /// </summary>
    public class LogRequestAndResponseMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Initialize middleware on startup
        /// </summary>
        /// <param name="next">Next middleware in the pipeline</param>
        public LogRequestAndResponseMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="loggingService"></param>
        /// <returns></returns>
        public virtual async Task InvokeAsync(HttpContext context, ILoggingService loggingService)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (loggingService == null) throw new ArgumentNullException(nameof(loggingService));

            string statusCode = null;
            string exception = null;

            var stream = context.Response.Body;
            MemoryStream RequestBuffer = new MemoryStream();
            await context.Request.Body.CopyToAsync(RequestBuffer)
                .ConfigureAwait(false);
            RequestBuffer.Position = 0; // rewind
            context.Request.Body = RequestBuffer;

            // Buffer the response
            MemoryStream ResponseBuffer = new MemoryStream();
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

            if (loggingService.LogCount > 0)
            {
                ContextLogParameters logParameters = new ContextLogParameters
                {
                    Context = context,
                    StatusCode = statusCode,
                    Exception = exception,
                    RequestBuffer = RequestBuffer,
                    ResponseBuffer = ResponseBuffer
                };

                loggingService.AddContextInfomation(logParameters);
                //Discard task to log off thread
                _ = loggingService.LogAll();
            }


            ResponseBuffer.Seek(0, SeekOrigin.Begin);
            await ResponseBuffer.CopyToAsync(stream).ConfigureAwait(false);
        }
    }
}
