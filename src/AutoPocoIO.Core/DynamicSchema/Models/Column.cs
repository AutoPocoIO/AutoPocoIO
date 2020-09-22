using System;

namespace AutoPocoIO.DynamicSchema.Models
{
    /// <summary>
    /// Details about a database column 
    /// </summary>
    public class Column
    {
        /// <summary>
        /// Parent table
        /// </summary>
        public Table Table { get; set; }
        /// <summary>
        /// Parent view
        /// </summary>
        public View View { get; set; }

        //Table
        /// <summary>
        /// Database schema name
        /// </summary>
        public string TableSchema { get; set; }
        /// <summary>
        /// Database object name
        /// </summary>
        public string TableName { get; set; }

        //Column
        /// <summary>
        /// Column name
        /// </summary>
        public string ColumnName { get; set; }
        /// <summary>
        /// Data type
        /// </summary>
        /// <example>Varchar</example>
        public string ColumnType { get; set; }
        /// <summary>
        /// Data length
        /// </summary>
        public int ColumnLength { get; set; }
        /// <summary>
        /// Does the column allow nulls
        /// </summary>
        public bool ColumnIsNullable { get; set; }
        /// <summary>
        /// Is the column a computed column
        /// </summary>
        public bool IsComputed { get; set; }

        //PK
        /// <summary>
        /// Name of the primary key, if the column is the primary key. 
        /// </summary>
        public string PKName { get; set; }

        /// <summary>
        /// Primary key order
        /// </summary>
        public int PKPosition { get; set; }

        /// <summary>
        /// Is the database responsible for creating the value on insert
        /// </summary>
        public bool PKIsIdentity { get; set; }

        //FK
        /// <summary>
        /// Name of the forign key, if the column is referenced by another table
        /// </summary>
        public string FKName { get; set; }

        /// <summary>
        /// Database of the forign keyed table
        /// </summary>
        public string ReferencedDatabase { get; set; }
        /// <summary>
        /// Schema of the forgien keyed table
        /// </summary>
        public string ReferencedSchema { get; set; }
        /// <summary>
        /// Name of the forgien keyed table
        /// </summary>
        public string ReferencedTable { get; set; }
        /// <summary>
        /// Name of the column forgien keyed 
        /// </summary>
        public string ReferencedColumn { get; set; }
        /// <summary>
        /// Forgien key name if defiend in the dashboard
        /// </summary>
        public string UserDefinedFKAlias { get; set; }

        /// <summary>
        /// Database and c# datatypes
        /// </summary>
        public DataType DataType { get; set; }

        /// <summary>
        /// Flag denoting if the column the Primary key
        /// </summary>
        public bool IsPK
        {
            get
            {
                return !string.IsNullOrEmpty(PKName);
            }
        }

        /// <summary>
        /// Flag denoting if the column is a forgien key
        /// </summary>
        public bool IsFK
        {
            get
            {
                return !string.IsNullOrEmpty(FKName);
            }
        }

        /// <summary>
        /// Flag the column to add the Browsable attribute
        /// </summary>
        public bool Browsable
        {
            get
            {
                return !((this.IsFK) || (this.IsPK && !this.PKIsIdentity));
            }
        }

        /// <summary>
        /// Full name of the forigen keyed table
        /// </summary>
        public string ReferencedVariableName
        {
            get
            {
                return $"{ReferencedDatabase}_{(string.IsNullOrEmpty(ReferencedSchema) ? "dbo" : ReferencedSchema)}_{ReferencedTable}";
            }
        }


        /// <summary>
        ///   A string that represents the current object.
        /// </summary>
        /// <returns>Column Name</returns>
        public override string ToString()
        {
            return ColumnName;
        }

        /// <summary>
        /// Combined hash codes of Schema, Table, Column and ColumnType
        /// </summary>
        /// <returns>A hash code for the current object</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                if (IsFK)
                    return HashCode.Combine(TableSchema, TableName, ColumnName, ColumnType);
                else
                    return HashCode.Combine(TableSchema, TableName, ColumnName, ColumnType, ReferencedVariableName);
            }
        }
    }
}
