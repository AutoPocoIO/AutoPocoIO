using System.Collections.Generic;

namespace AutoPocoIO.Middleware
{
    public interface ILayoutPage
    {
        IDictionary<string, string> Sections { get; }

        void Execute();
        void Assign(RazorPage razorPage);
        string TransformText(string content);
    }
}
