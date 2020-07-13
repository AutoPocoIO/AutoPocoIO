using AutoPocoIO.Exceptions;
using AutoPocoIO.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Text;


namespace AutoPocoIO.Middleware
{
    /// <summary>
    /// Base page for all middleware pages.
    /// </summary>
    public abstract class RazorPage
    {
        private readonly StringBuilder _content;
        private string _body;
        
        /// <summary>
        /// Initialize the page base.
        /// </summary>
        protected RazorPage()
        {
            ViewBag = new ExpandoObject();
            Sections = new Dictionary<string, string>();
            _content = new StringBuilder();
        }

        /// <summary>
        /// Initialize the page base with a layout.
        /// </summary>
        /// <param name="layout">Unified layout.</param>
        /// <param name="title">Page title.</param>
        protected RazorPage(ILayoutPage layout, string title) : this()
        {
            Check.NotNull(layout, nameof(layout));

            Layout = layout;
            Layout.Title = title;
        }
        /// <summary>
        /// 
        /// </summary>
        public abstract void Execute();

        /// <summary>
        /// Get or set page layout
        /// </summary>
        public ILayoutPage Layout { get; protected set; }
        /// <summary>
        /// Get or set request context
        /// </summary>
        public IMiddlewareContext Context { get; private set; }
        /// <summary>
        /// Get or set http request
        /// </summary>
        public IMiddlewareRequest Request => Context.Request;
        /// <summary>
        /// Get or set request scoped logging service
        /// </summary>
        public ILoggingService  LoggingService { get; set; }
        /// <summary>
        /// Get html render sections
        /// </summary>
        public IDictionary<string, string> Sections { get; }
        /// <summary>
        /// Get view bag
        /// </summary>
        public IDictionary<string, object> ViewBag { get; }
        /// <summary>
        /// Transform page to html.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return TransformText(null);
        }
        /// <summary>
        /// Assign parent context to page.
        /// </summary>
        /// <param name="parentPage">Parent razor page.</param>
        public virtual void Assign(RazorPage parentPage)
        {
            Check.NotNull(parentPage, nameof(parentPage));

            Context = parentPage.Context;
        }

        /// <summary>
        /// Execute html render
        /// </summary>
        /// <param name="body">Page body if using a layout.</param>
        /// <returns></returns>
        public string TransformText(string body)
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public virtual void Assign(IMiddlewareContext context)
        {
            Context = context;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual object RenderBody()
        {
            return new NonEscapedString(_body);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="scriptTag"></param>
        /// <returns></returns>
        protected virtual object RenderSection(string scriptTag)
        {
            if (Sections.ContainsKey(scriptTag))
                return new NonEscapedString(Sections[scriptTag]);
            else
                return new NonEscapedString(string.Empty);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="scriptTag"></param>
        /// <param name="renderAction"></param>
        protected virtual void DefineSection(string scriptTag, Action renderAction)
        {
            Check.NotNull(scriptTag, nameof(scriptTag));
            Check.NotNull(renderAction, nameof(renderAction));

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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="textToAppend"></param>
        protected void WriteLiteral(string textToAppend)
        {
            if (string.IsNullOrEmpty(textToAppend))
                return;
            _content.Append(textToAppend);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1054:Uri parameters should not be strings", Justification = "<Pending>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1055:Uri return values should not be strings", Justification = "<Pending>")]
        protected string TransformUrl(string url)
        {
            Check.NotNull(url, nameof(url));
            return Context.Request.PathBase +  url;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="prefix"></param>
        /// <param name="suffix"></param>
        /// <param name="fragments"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Required by razor generator")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1801:Remove unused parameter", Justification = "Required by razor generator")]
        protected void WriteAttribute(string name, Tuple<string, int> prefix, Tuple<string, int> suffix, params object[] fragments)
        {
            Check.NotNull(name, nameof(name));
            Check.NotNull(prefix, nameof(prefix));
            Check.NotNull(suffix, nameof(suffix));

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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        protected virtual void Write(object value)
        {
            if (value == null)
                return;

            var html = value as NonEscapedString;
            WriteLiteral(html?.ToString() ?? Encode(value.ToString()));
        }
       

      /// <summary>
      /// 
      /// </summary>
      /// <param name="list"></param>
      /// <returns></returns>
        protected static NonEscapedString TransformArray(object list)
        {
            Check.NotNull(list, nameof(list));

            if (list is string[] stringList)
                return TransformArray(stringList);
            else if (list is int[] intList)
                return TransformArray(intList);
            else
                return new NonEscapedString(list.ToString());

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        protected static NonEscapedString TransformArray(string[] list)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("[");
            builder.Append(string.Join(",", list.Select(c => $"'{c}'")));
            builder.Append("]");

            return new NonEscapedString(builder.ToString());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        protected static NonEscapedString TransformArray(int[] list)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("[");
            builder.Append(string.Join(",", list));
            builder.Append("]");

            return new NonEscapedString(builder.ToString());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        protected T GetViewBagValue<T>(string key) where T: class
        {
            return ViewBag[key] as T;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorName"></param>
        /// <param name="errorKey"></param>
        /// <returns></returns>
        protected string GetError(string errorName, string errorKey) 
        {
            if (ViewBag[errorName] is IDictionary<string, string> errors && errors.TryGetValue(errorKey, out string errorMessage))
                return errorMessage;
            else
                return string.Empty;
        }

        private static string Encode(string text)
        {
            return string.IsNullOrEmpty(text) ? string.Empty : WebUtility.HtmlEncode(text);
        }
    }
}