﻿using AutoPocoIO.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AutoPocoIO.Middleware.Dispatchers
{
    internal class RazorPageDispatcher<TPage> : IMiddlewareDispatcher where TPage : RazorPage
    {
        private readonly Action<TPage, Match> _pageFunc;

        public RazorPageDispatcher(Action<TPage, Match> pageFunc)
        {
            _pageFunc = pageFunc;
        }

        public Task Dispatch(IMiddlewareContext context, ILoggingService logginerService)
        {
            context.Response.ContentType = "text/html";

            TPage page = context.InternalServiceProvider.GetRequiredService<TPage>();
            page.Assign(context);
            _pageFunc(page, context.UriMatch);


            return context.Response.WriteAsync(page.ToString());
        }
    }
}
