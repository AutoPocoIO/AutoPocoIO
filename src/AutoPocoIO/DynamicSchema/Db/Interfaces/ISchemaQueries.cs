namespace AutoPocoIO.DynamicSchema.Db
{
    /// <summary>
    /// Methods to pull query for schema information from a database
    /// </summary>
    public interface ISchemaQueries
    {
        /// <summary>
        /// List all requested database tables, views, and columns.
        /// </summary>
        /// <returns>string database query for tables, views, and columns.</returns>
        string BuildColumns();

        /// <summary>
        /// List all stored procedures
        /// </summary>
        /// <returns>string database query for stored procedures</returns>
        string BuildStoredProcedureCommand();

        /// <summary>
        /// List all requested database tables, views.
        /// </summary>
        /// <returns>string database query for tables, views.</returns>
        string BuildTablesViewCommand();
    }
}