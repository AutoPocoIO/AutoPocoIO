using AutoPocoIO.DynamicSchema.Models;
using System;
using System.Collections.Generic;

namespace AutoPocoIO.DynamicSchema.Db
{
    /// <summary>
    /// Schema definition at request time
    /// </summary>
    public class DbSchema : IDbSchema
    {
        /// <summary>
        /// Initalize schmea definition lists.
        /// </summary>
        public DbSchema()
        {
            Tables = new List<Table>();
            Columns = new List<Column>();
            Views = new List<View>();
            StoredProcedures = new List<StoredProcedure>();
            SchemaNames = new List<string>();
        }

        ///<inheritdoc/>
        public virtual List<Table> Tables { get; }
        ///<inheritdoc/>
        public virtual List<Column> Columns { get; }
        ///<inheritdoc/>
        public virtual List<View> Views { get; }
        ///<inheritdoc/>
        public virtual List<StoredProcedure> StoredProcedures { get; }
        ///<inheritdoc/>
        public virtual List<string> SchemaNames { get; }

        /// <summary>
        /// Overall hash code for the request
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            var hash = new HashCode();

            SortedSet<int> objectHashes = new SortedSet<int>();

            Tables.ForEach(c => objectHashes.Add(c.GetHashCode()));
            Views.ForEach(c => objectHashes.Add(c.GetHashCode()));
            StoredProcedures.ForEach(c => objectHashes.Add(c.GetHashCode()));

            foreach (var objHash in objectHashes)
                hash.Add(objHash);

            return hash.ToHashCode();
        }

        ///<inheritdoc/>
        public void Reset()
        {
            Tables.Clear();
            Columns.Clear();
            Views.Clear();
            StoredProcedures.Clear();
            SchemaNames.Clear();
        }
    }
}
