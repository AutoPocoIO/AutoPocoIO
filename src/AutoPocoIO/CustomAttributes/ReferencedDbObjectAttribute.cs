using System;

namespace AutoPocoIO.CustomAttributes
{
    /// <summary>
    /// Specifies the referenced Database Object of a property
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ReferencedDbObjectAttribute : Attribute
    {
        /// <summary>
        /// Database name of the referenced database object
        /// </summary>
        public string DbName { get; private set; }
        /// <summary>
        /// Schema name of the referenced database object
        /// </summary>
        public string SchemaName { get; private set; }
        /// <summary>
        /// Table name of the referenced database object
        /// </summary>
        public string TableName { get; private set; }
        /// <summary>
        /// Column name (if referenced object is a column, null otherwise) of the referenced database object
        /// </summary>
        public string ColumnName { get; private set; }

        /// <summary>
        /// Initializes a new instance of the AutoPoco.CustomAttributes.ReferencedDbObjectAttribute class (use when referenced object is a table, not a specific column)
        /// </summary>
        /// <param name="dbName">Database name of the referenced database object</param>
        /// <param name="schemaName">Schema name of the referenced database object</param>
        /// <param name="tableName">Table name of the referenced database object</param>
        public ReferencedDbObjectAttribute(string dbName, string schemaName, string tableName)
        {
            DbName = dbName;
            SchemaName = schemaName;
            TableName = tableName;
        }

        /// <summary>
        /// Initializes a new instance of the AutoPoco.CustomAttributes.ReferencedDbObjectAttribute class (use when referenced object is a specific column)
        /// </summary>
        /// <param name="dbName">Database name of the referenced database object</param>
        /// <param name="schemaName">Schema name of the referenced database object</param>
        /// <param name="tableName">Table name of the referenced database object</param>
        /// <param name="columnName">Column name of the referenced database object</param>
        public ReferencedDbObjectAttribute(string dbName, string schemaName, string tableName, string columnName)
        {
            DbName = dbName;
            SchemaName = schemaName;
            TableName = tableName;
            ColumnName = columnName;
        }
    }
}
