using System.Collections.Generic;

namespace AutoPocoIO.Models
{
    /// <summary>
    /// Database table information
    /// </summary>
    public class TableDefinition
    {
        /// <summary>
        /// AutoPoco connector id
        /// </summary>
        public int ConnectorId { get; set; }

        /// <summary>
        /// AutoPoco connector name
        /// </summary>
        public string ConnectorName { get; set; }

        /// <summary>
        /// Database schema name
        /// </summary>
        public string SchemaName { get; set; }

        /// <summary>
        /// Database name
        /// </summary>
        public string DbName { get; set; }

        /// <summary>
        /// Table name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// List of table column details
        /// </summary>
        public IEnumerable<ColumnDefinition> Columns { get; set; }
    }
}
