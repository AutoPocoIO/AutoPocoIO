using AutoPocoIO.DynamicSchema.Models;
using System;
using System.Collections.Generic;

namespace AutoPocoIO.DynamicSchema.Db
{
    public class DbSchema : IDbSchema
    {

        public DbSchema()
        {
            Tables = new List<Table>();
            Columns = new List<Column>();
            Views = new List<View>();
            StoredProcedures = new List<StoredProcedure>();
        }

        public virtual List<Table> Tables { get; }
        public virtual List<Column> Columns { get; }
        public virtual List<View> Views { get; }
        public virtual List<StoredProcedure> StoredProcedures { get; }

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

        public void Reset()
        {
            Tables.Clear();
            Columns.Clear();
            Views.Clear();
            StoredProcedures.Clear();
        }
    }
}
