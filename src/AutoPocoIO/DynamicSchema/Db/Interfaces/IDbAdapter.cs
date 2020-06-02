using System;
using System.Linq;

namespace AutoPocoIO.DynamicSchema.Db
{
    public interface IDbAdapter
    {
        IDbContextBase Instance { get; }
        Type DbSetEntityType { get; }


        void Add(object value);
        void Delete(dynamic value);
        object Find(string tableName, string keys);
        IQueryable<object> FilterByKey(string tableName, string keys);
        object GetAll(string tableName);

        object NewInstance(string tableName);
        int Save();
        void SetupDataContext(string tableName);
        object GetWithoutContext(string tableName, string outerTableName);

        void Update(object value);
        IQueryable<T> FilterById<T>(object id) where T : class;
    }
}