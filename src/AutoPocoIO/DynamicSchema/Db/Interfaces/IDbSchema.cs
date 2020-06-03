using AutoPocoIO.DynamicSchema.Models;
using System.Collections.Generic;

namespace AutoPocoIO.DynamicSchema.Db
{
    public interface IDbSchema
    {
        void Reset();
        List<Column> Columns { get; }
        List<StoredProcedure> StoredProcedures { get; }
        List<Table> Tables { get; }
        List<View> Views { get; }
    }
}