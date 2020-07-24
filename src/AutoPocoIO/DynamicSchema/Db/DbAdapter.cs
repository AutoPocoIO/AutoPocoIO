using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.DynamicSchema.Runtime;
using AutoPocoIO.DynamicSchema.Util;
using AutoPocoIO.Exceptions;
using AutoPocoIO.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoPocoIO.DynamicSchema.Db
{
    internal class DbAdapter : IDisposable, IDbAdapter
    {
        private bool _disposed;


        private readonly DynamicClassBuilder _dynamicClassBuilder;
        private readonly IDbSchemaBuilder _dbSchemaBuilder;
        private readonly IDbSchema _dbSchema;

        public DbAdapter(IDbSchemaBuilder schemBuilder, DynamicClassBuilder classBuilder, IDbSchema dbSchema)
        {
            Check.NotNull(schemBuilder, nameof(schemBuilder));
            Check.NotNull(classBuilder, nameof(classBuilder));
            Check.NotNull(dbSchema, nameof(dbSchema));

            _dbSchemaBuilder = schemBuilder;
            _dynamicClassBuilder = classBuilder;
            _dbSchema = dbSchema;
        }


        public virtual IDbContextBase Instance { get; private set; }

        public virtual Type DbSetEntityType { get; private set; }

        public virtual void SetupDataContext(string tableName)
        {
            var table = _dbSchema.Tables.Find(x => x.VariableName == tableName) ?? _dbSchema.Views.Find(x => x.VariableName == tableName);
            _dynamicClassBuilder.CreateModelTypes(tableName);

            var dbContextOptions = _dbSchemaBuilder.CreateDbContextOptions();
            Instance = (DbContextBase)Activator.CreateInstance(typeof(DbContextBase),
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null,
                new object[] { dbContextOptions, _dynamicClassBuilder.ExistingAssemblies, _dbSchema.Tables },
                null);

            //Entity DbSet
            var assemblyName = Utils.AssemblyName(table, tableName, _dbSchema.GetHashCode());
            DbSetEntityType = _dynamicClassBuilder.ExistingAssemblies.Where(x => x.Key.StartsWith(assemblyName, StringComparison.InvariantCultureIgnoreCase)).Single().Value;
        }

        public virtual IDbCommand CreateDbCommand() => _dbSchemaBuilder.CreateConnection().CreateCommand();

        public object GetAll(string tableName)
        {
            SetupDataContext(tableName);
            return GetDbSet();
        }
        public object GetWithoutContext(string tableName, string outerTableName)
        {
            var table = _dbSchema.Tables.Find(x => x.VariableName == tableName) ?? _dbSchema.Views.Find(x => x.VariableName == tableName);
            var assemblyName = Utils.AssemblyName(table, outerTableName, _dbSchema.GetHashCode());

            var dbSetEntity = _dynamicClassBuilder.ExistingAssemblies.Where(x => x.Key.Equals(assemblyName, StringComparison.InvariantCultureIgnoreCase)).Single().Value;

            var dbTable = typeof(DbContextBase).GetMethod("Set", Array.Empty<Type>()).MakeGenericMethod(dbSetEntity).InvokeWithException(Instance, null);

            return dbTable;
        }

        public object GetDbSet()
        {
            var getDbSetMethod = typeof(DbAdapter).GetMethod(nameof(GetDbSet), BindingFlags.Instance | BindingFlags.NonPublic);
            var dbset = getDbSetMethod.MakeGenericMethod(new Type[] { DbSetEntityType }).InvokeWithException(this, null);

            return dbset;
        }

        public object Find(string tableName, string keys)
        {
            SetupDataContext(tableName);

            var getByIdMethod = typeof(DbAdapter).GetMethod(nameof(GetById), BindingFlags.Instance | BindingFlags.NonPublic);
            var singleRecord = getByIdMethod.MakeGenericMethod(new Type[] { DbSetEntityType }).InvokeWithException(this, new object[] { keys });

            return singleRecord;
        }

        public IQueryable<object> FilterByKey(string tableName, string keys)
        {
            SetupDataContext(tableName);

            var getByIdMethod = typeof(DbAdapter).GetMethod(nameof(FilterById), BindingFlags.Instance | BindingFlags.NonPublic);
            var filteredList = getByIdMethod.MakeGenericMethod(new Type[] { DbSetEntityType }).InvokeWithException(this, new object[] { keys });

            return (IQueryable<object>)filteredList;
        }

        public object NewInstance(string tableName)
        {
            SetupDataContext(tableName);
            return Activator.CreateInstance(DbSetEntityType);
        }

        public void Add(object obj)
        {
            var insertMethod = typeof(DbAdapter).GetMethod(nameof(InsertRecord), BindingFlags.Instance | BindingFlags.NonPublic);
            insertMethod.MakeGenericMethod(new Type[] { DbSetEntityType }).InvokeWithException(this, new object[] { obj });
        }

        public void Update(object obj)
        {
            var insertMethod = typeof(DbAdapter).GetMethod(nameof(UpdateRecord), BindingFlags.Instance | BindingFlags.NonPublic);
            insertMethod.MakeGenericMethod(new Type[] { DbSetEntityType }).InvokeWithException(this, new object[] { obj });

        }

        public void Delete(object obj)
        {
            var deleteMethod = typeof(DbAdapter).GetMethod(nameof(DeleteRecord), BindingFlags.Instance | BindingFlags.NonPublic);
            deleteMethod.MakeGenericMethod(new Type[] { DbSetEntityType }).InvokeWithException(this, new object[] { obj });
        }

        public IEnumerable<PrimaryKeyInformation> MapPrimaryKey(object obj)
        {
            var keyMethod = typeof(DbAdapter).GetMethod(nameof(GetPrimaryKeyInformation), BindingFlags.Instance | BindingFlags.NonPublic);
            return (IEnumerable<PrimaryKeyInformation>)keyMethod.MakeGenericMethod(new Type[] { DbSetEntityType, obj.GetType() }).InvokeWithException(this, new object[] { obj });
        }

        public int Save()
        {
            return Instance.SaveChanges();
        }

        private IEnumerable<PrimaryKeyInformation> GetPrimaryKeyName<T>()
        {
            var type = Instance.Model.FindEntityType(typeof(T));

            var PK = (from m in type.FindPrimaryKey().Properties
                      where m.IsPrimaryKey()
                      select new PrimaryKeyInformation { Name = m.Name, Type = m.FieldInfo.FieldType });
            return PK;
        }

        private IEnumerable<PrimaryKeyInformation> GetPrimaryKeyInformation<TDbSet, TModel>(TModel value)
            where TDbSet : class
            where TModel : class
        {
            return GetPrimaryKeyName<TDbSet>()
                   .ToList()
                   .GetTableKeys<TDbSet, TModel>(value);
        }

        private DbSet<T> GetDbSet<T>() where T : class
        {
            return (DbSet<T>)typeof(DbContextBase).GetMethod("Set", Array.Empty<Type>()).MakeGenericMethod(DbSetEntityType).InvokeWithException(Instance, null);
        }

        private void InsertRecord<T>(object record) where T : class
        {
            DbSet<T> table = GetDbSet<T>();
            table.Add((T)record);
        }

        private void UpdateRecord<T>(object record) where T : class
        {
            DbSet<T> table = GetDbSet<T>();
            table.Update((T)record);
        }

        private void DeleteRecord<T>(object record) where T : class
        {
            DbSet<T> table = GetDbSet<T>();
            table.Remove((T)record);
        }

        private T GetById<T>(object id) where T : class
        {
            IQueryable<T> table = FilterById<T>(id);
            return table.Single();
        }

        private IQueryable<T> FilterById<T>(object id) where T : class
        {
            var itemParameter = Expression.Parameter(typeof(T), "item");
            var PKs = GetPrimaryKeyName<T>().ToArray()
                        .GetTableKeys(id.ToString());

            IQueryable<T> table = GetDbSet<T>();

            foreach (var PK in PKs)
            {
                var whereExpression = Expression.Lambda<Func<T, bool>>
                    (
                    Expression.Equal(
                        Expression.Property(
                            itemParameter,
                            PK.Name
                            ),
                        Expression.Constant(PK.Value, PK.Type)
                        ),
                    new[] { itemParameter }
                    );

                table = table.Where(whereExpression);
            }

            return table;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Instance = null;
                }
                //release unmanaged resources.
            }
            _disposed = true;
        }


    }
}
