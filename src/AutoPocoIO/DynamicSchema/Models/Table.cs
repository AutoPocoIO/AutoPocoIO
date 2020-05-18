using System;
using System.Collections.Generic;

namespace AutoPocoIO.DynamicSchema.Models
{
    public class Table
    {
        public string Database { get; set; }
        public string Schema { get; set; }
        public string Name { get; set; }
        public string PrimaryKeys { get; set; }

        public virtual List<Column> Columns { get; }

        public virtual string VariableName
        {
            get
            {
                return $"{Database}_{(string.IsNullOrEmpty(Schema) ? "dbo" : Schema)}_{Name}";
            }
        }

        public virtual string TableAttributeName => Name;


        public Table()
        {
            Columns = new List<Column>();
        }

        public override string ToString()
        {
            return VariableName;
        }

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
