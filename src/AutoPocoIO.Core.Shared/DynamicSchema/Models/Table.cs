using System;
using System.Collections.Generic;

namespace AutoPocoIO.DynamicSchema.Models
{
    /// <summary>
    /// Table details
    /// </summary>
    public class Table
    {
        /// <summary>
        /// Initialize table column list.
        /// </summary>
        public Table()
        {
            Columns = new List<Column>();
        }

        /// <summary>
        /// Database
        /// </summary>
        public string Database { get; set; }
        /// <summary>
        /// Database schema
        /// </summary>
        public string Schema { get; set; }
        /// <summary>
        /// Table name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Table primary keys
        /// </summary>
        public string PrimaryKeys { get; set; }
        /// <summary>
        /// List of table columns
        /// </summary>
        public virtual List<Column> Columns { get; }
        /// <summary>
        /// Combine table database, schema and name
        /// </summary>
        public virtual string VariableName
        {
            get
            {
                return $"{Database}_{(string.IsNullOrEmpty(Schema) ? "dbo" : Schema)}_{Name}";
            }
        }

        /// <summary>
        /// Table name
        /// </summary>
        public virtual string TableAttributeName => Name;

        /// <summary>
        /// Name of table
        /// </summary>
        /// <returns>Variable name</returns>
        public override string ToString()
        {
            return VariableName;
        }

        /// <summary>
        /// Orderd has of columns
        /// </summary>
        /// <returns>An order independent hash.</returns>
        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(VariableName);

            SortedSet<int> colHashes = new SortedSet<int>();
            Columns.ForEach(c => colHashes.Add(c.GetHashCode()));

            foreach (var colHash in colHashes)
                hash.Add(colHash);

            return hash.ToHashCode();
        }
    }
}
