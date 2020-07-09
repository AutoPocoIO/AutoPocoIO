using AutoPocoIO.DynamicSchema.Models;
using System.Collections.Generic;

namespace AutoPocoIO.DynamicSchema.Db
{
    /// <summary>
    /// Schema definition at request time
    /// </summary>
    public interface IDbSchema
    {
        /// <summary>
        /// Clear <see cref="Columns"/>, <see cref="Tables"/>, <see cref="Views"/>, and <see cref="StoredProcedures"/>
        /// </summary>
        void Reset();
        /// <summary>
        /// List of found columns from all objects
        /// </summary>
        List<Column> Columns { get; }
        /// <summary>
        /// Stored procedures found
        /// </summary>
        List<StoredProcedure> StoredProcedures { get; }
        /// <summary>
        /// Tables found.
        /// </summary>
        List<Table> Tables { get; }
        /// <summary>
        /// Views found.
        /// </summary>
        List<View> Views { get; }
    }
}