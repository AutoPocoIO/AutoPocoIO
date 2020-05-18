namespace AutoPocoIO.Exceptions
{
    internal class StoreProcedureNotFoundException : BaseCaughtException
    {
        private readonly string _schemaName;
        private readonly string _sprocName;
        public StoreProcedureNotFoundException(string schemaName, string sprocName) : base()
        {
            _schemaName = schemaName;
            _sprocName = sprocName;
        }
        public override string Message => $"Stored Procedure '{_schemaName}.{_sprocName}' not found.";
        public override string HttpErrorMessage => "StoreProcedureNotFound";
    }
}