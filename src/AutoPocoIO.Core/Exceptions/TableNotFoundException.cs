namespace AutoPocoIO.Exceptions
{
    /// <summary>
    /// Exception that sets status to 500 (Server Error)
    /// </summary>
    public class TableNotFoundException : BaseCaughtException
    {
        private readonly string _schemaName;
        private readonly string _tableName;
        private readonly string _databaseName;

        /// <summary>
        /// Initialize exception table information
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="schemaName"></param>
        /// <param name="tableName"></param>
        public TableNotFoundException(string databaseName, string schemaName, string tableName) : base()
        {
            _databaseName = databaseName;
            _schemaName = schemaName;
            _tableName = tableName;
        }

        /// <inheritdoc/>
        public override string Message => $"Table '{_databaseName}.{_schemaName}.{_tableName}' not found.";
        /// <summary>
        /// Status message is <c>TableNotFound</c>
        /// </summary>
        public override string HttpErrorMessage => "TableNotFound";
    }
}