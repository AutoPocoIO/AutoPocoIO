namespace AutoPocoIO.Exceptions
{
    public class TableNotFoundException : BaseCaughtException
    {
        private readonly string _schemaName;
        private readonly string _tableName;
        private readonly string _databaseName;

        public TableNotFoundException(string databaseName, string schemaName, string tableName) : base()
        {
            _databaseName = databaseName;
            _schemaName = schemaName;
            _tableName = tableName;
        }
        public override string Message => $"Table '{_databaseName}.{_schemaName}.{_tableName}' not found.";
        public override string HttpErrorMessage => "TableNotFound";
    }
}