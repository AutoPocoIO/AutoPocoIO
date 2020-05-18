﻿using AutoPocoIO.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Text;


namespace AutoPocoIO.Middleware
{
    internal abstract class RazorPage
    {
        private readonly StringBuilder _content = new StringBuilder();
        private string _body;

        protected RazorPage()
        {
            ViewBag = new ExpandoObject();
        }

        public RazorPage Layout { get; protected set; }
     //   public UrlHelper Url { get; private set; }

        internal IMiddlewareContext Context { get; private set; }

        internal IMiddlewareRequest Request => Context.Request;
        internal IMiddlewareResponse Response => Context.Response;

        public IDictionary<string, object> ViewBag { get; }
        public string RequestPath => Request.Path;

        protected ILoggingService LoggingService { get; set; }

        public abstract void Execute();

        public string Query(string key)
        {
            return Request.GetQuery(key);
        }

        public override string ToString()
        {
            return TransformText(null);
        }

        public void Assign(RazorPage parentPage)
        {
            Context = parentPage.Context;
         //   Url = parentPage.Url;
        }

        internal void Assign(IMiddlewareContext context)
        {
            Context = context;
         //   Url = new UrlHelper(context);
        }

        protected void WriteLiteral(string textToAppend)
        {
            if (string.IsNullOrEmpty(textToAppend))
                return;
            _content.Append(textToAppend);
        }

        protected string TransmformUrl(string url)
        {
            return Context.Request.PathBase +  url;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Required by razor generator")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1801:Remove unused parameter", Justification = "Required by razor generator")]
        protected void WriteAttribute(string name, Tuple<string, int> prefix, Tuple<string, int> suffix, params object[] fragments)
        {

            if (fragments.Length == 0)
            {
                WriteLiteral(prefix.Item1);
                WriteLiteral(suffix.Item1);
            }
            else
            {
                bool first = true;
                bool wroteSomething = false;

                foreach (var fragment in fragments)
                {
                    var sf = fragment as Tuple<Tuple<string, int>, Tuple<string, int>, bool>;
                    var of = fragment as Tuple<Tuple<string, int>, Tuple<object, int>, bool>;

                    string ws = sf != null ? sf.Item1.Item1 : of?.Item1.Item1;

                    if (first)
                    {
                        WriteLiteral(prefix.Item1);
                        first = false;
                    }
                    else
                    {
                        WriteLiteral(ws);
                    }


                    if (sf != null)
                        WriteLiteral(sf.Item2.Item1);
                    else if (of != null)
                        Write(of.Item2.Item1);

                    wroteSomething = true;

                }

                if (wroteSomething)
                    WriteLiteral(suffix.Item1);
            }
        }

        protected virtual void Write(object value)
        {
            if (value == null)
                return;

            var html = value as NonEscapedString;
            WriteLiteral(html?.ToString() ?? Encode(value.ToString()));
        }

        protected virtual object RenderBody()
        {
            return _body;
        }

        private string TransformText(string body)
        {
            _body = body;
            Execute();

            if (Layout != null)
            {
                Layout.Assign(this);
                return Layout.TransformText(_content.ToString());
            }

            return _content.ToString();
        }

        private static string Encode(string text)
        {
            return string.IsNullOrEmpty(text) ? string.Empty : WebUtility.HtmlEncode(text);
        }

        protected NonEscapedString TransformArray(object list)
        {
            if (list is string[] stringList)
                return TransformArray(stringList);
            else if (list is int[] intList)
                return TransformArray(intList);
            else
                return new NonEscapedString(list.ToString());

        }
        protected NonEscapedString TransformArray(string[] list)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("[");
            builder.Append(string.Join(",", list.Select(c => $"'{c}'")));
            builder.Append("]");

            return new NonEscapedString(builder.ToString());
        }

        protected NonEscapedString TransformArray(int[] list)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("[");
            builder.Append(string.Join(",", list));
            builder.Append("]");

            return new NonEscapedString(builder.ToString());
        }
    }
}
