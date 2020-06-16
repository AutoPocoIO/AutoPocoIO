using AutoPocoIO.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Text;


namespace AutoPocoIO.Middleware
{
    public abstract class RazorPage
    {
        private readonly StringBuilder _content;
        private string _body;
        

        protected RazorPage()
        {
            ViewBag = new ExpandoObject();
            Sections = new Dictionary<string, string>();
            _content = new StringBuilder();
        }

        public RazorPage Layout { get; protected set; }

        public IMiddlewareContext Context { get; private set; }
        public IMiddlewareRequest Request => Context.Request;
        public IMiddlewareResponse Response => Context.Response;
        public ILoggingService  LoggingService { get; set; }
        public IDictionary<string, string> Sections { get; }
        public IDictionary<string, object> ViewBag { get; }
        public string RequestPath => Request.Path;

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
        }

        public void Assign(IMiddlewareContext context)
        {
            Context = context;
        }

        protected virtual object RenderBody()
        {
            return new NonEscapedString(_body);
        }

        protected virtual object RenderSection(string scriptTag)
        {
            if (Sections.ContainsKey(scriptTag))
                return new NonEscapedString(Sections[scriptTag]);
            else
                return new NonEscapedString(string.Empty);
        }

        protected virtual void DefineSection(string scriptTag, Action renderAction)
        {
            var bufferContent = new StringBuilder();
            bufferContent.Append(_content.ToString());

            _content.Clear();
            renderAction.Invoke();
            string section = _content.ToString();

            if (Layout != null)
                Layout.Sections[scriptTag] = section;

            _content.Clear();
            _content.Append(bufferContent.ToString());

        }

        protected void WriteLiteral(string textToAppend)
        {
            if (string.IsNullOrEmpty(textToAppend))
                return;
            _content.Append(textToAppend);
        }

        protected string TransformUrl(string url)
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

        protected string TransformText(string body)
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
        protected static NonEscapedString TransformArray(string[] list)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("[");
            builder.Append(string.Join(",", list.Select(c => $"'{c}'")));
            builder.Append("]");

            return new NonEscapedString(builder.ToString());
        }

        protected static NonEscapedString TransformArray(int[] list)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("[");
            builder.Append(string.Join(",", list));
            builder.Append("]");

            return new NonEscapedString(builder.ToString());
        }

        protected T GetViewBagValue<T>(string key) where T: class
        {
            return ViewBag[key] as T;
        }

        protected string GetError(string errorName, string errorKey) 
        {
            if (ViewBag[errorName] is IDictionary<string, string> errors && errors.TryGetValue(errorKey, out string errorMessage))
                return errorMessage;
            else
                return string.Empty;
        }
    }
}
