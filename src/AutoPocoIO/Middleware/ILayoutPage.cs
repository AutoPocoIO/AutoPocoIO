using System.Collections.Generic;

namespace AutoPocoIO.Middleware
{
    public interface ILayoutPage
    {
        IDictionary<string, string> Sections { get; }
        string Title { get; set; }

        void Execute();
        void Assign(RazorPage razorPage);
        string TransformText(string content);
    }
}
