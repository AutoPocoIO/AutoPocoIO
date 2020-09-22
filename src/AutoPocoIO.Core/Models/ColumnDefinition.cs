using Newtonsoft.Json;

namespace AutoPocoIO.Models
{
    /// <summary>
    /// Database column information
    /// </summary>
    public class ColumnDefinition
    {
        /// <summary>
        /// Column Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Database type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Max length of the column
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// Does the column allow null values
        /// </summary>
        public bool IsNullable { get; set; }

        /// <summary>
        /// Is the column the primary key or one of the primary keys
        /// </summary>
        public bool IsPrimaryKey { get; set; }

        /// <summary>
        /// Is attached to a forigen key constrant
        /// </summary>
        public bool IsForigenKey { get; internal set; }

        /// <summary>
        /// Forigen key table's database name
        /// </summary>
        public string ReferencedDatabase { get; set; }

        /// <summary>
        /// Forigen key table's schema name
        /// </summary>
        public string ReferencedSchema { get; set; }

        /// <summary>
        /// Forigen key table name
        /// </summary>
        public string ReferencedTable { get; set; }

        /// <summary>
        /// Forigen key column
        /// </summary>
        public string ReferencedColumn { get; set; }

        /// <summary>
        /// Is the column definition a computed column
        /// </summary>
        public bool IsComputed { get; set; }

        /// <summary>
        /// Flag to specify that the column is a primary key identiy column
        /// </summary>
        [JsonIgnore]
        internal bool IsPrimaryKeyIdentity { get; set; }
    }
}
