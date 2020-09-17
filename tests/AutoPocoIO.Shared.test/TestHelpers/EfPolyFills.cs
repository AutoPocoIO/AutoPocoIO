#if EF31

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;

namespace AutoPocoIO.test
{
    public static class EfPolyFills
    {
        public static IIndexFill Relational(this IIndex index)
        {
            return new IIndexFill(index);
        }

        public static IPropertyFill Relational(this IProperty property)
        {
            return new IPropertyFill(property);
        }

        public static IEntityTypeFill Relational(this IEntityType entityType)
        {
            return new IEntityTypeFill(entityType);
        }


    }

    public class IIndexFill
    {
        private IIndex index;

        public IIndexFill(IIndex index)
        {
            this.index = index;
        }

        public string Name => index.GetName();

        public string Filter => index.GetFilter();
    }

    public class IEntityTypeFill
    {
        private IEntityType entityType;

        public IEntityTypeFill(IEntityType entityType)
        {
            this.entityType = entityType;
        }

        public string Schema => entityType.GetSchema();

        public string TableName => entityType.GetTableName();
    }

    public class IPropertyFill
    {
        private IProperty property;

        public IPropertyFill(IProperty property)
        {
            this.property = property;
        }

        public string ColumnType => property.GetColumnType();

        public string ComputedColumnSql => property.GetComputedColumnSql();
    }

}
#endif

#if NETCORE3_1 && EF31

namespace Microsoft.AspNetCore.Http.Internal
{
    public class QueryCollection : Microsoft.AspNetCore.Http.QueryCollection
    {
        public QueryCollection()
        {
        }

        public QueryCollection(Http.QueryCollection store) : base(store)
        {
        }

        public QueryCollection(Dictionary<string, StringValues> store) : base(store)
        {
        }

        public QueryCollection(int capacity) : base(capacity)
        {
        }
    }
}
#endif

