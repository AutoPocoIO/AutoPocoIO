namespace AutoPocoIO.Exceptions
{
    internal class ViewNotFoundException : BaseCaughtException
    {
        private readonly string _schemaName;
        private readonly string _viewName;

        public ViewNotFoundException(string schemaName, string viewName) : base()
        {
            _schemaName = schemaName;
            _viewName = viewName;
        }
        public override string Message => $"View '{_schemaName}.{_viewName}' not found.";
        public override string HttpErrorMessage => "ViewNotFound";
    }
}