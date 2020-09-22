using AutoPocoIO.DynamicSchema.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace AutoPocoIO.DynamicSchema.Db
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDbAdapter
    {
        /// <summary>
        /// Database instance
        /// </summary>
        IDbContextBase Instance { get; }
        /// <summary>
        /// Type of requested database object
        /// </summary>
        Type DbSetEntityType { get; }

        /// <summary>
        /// Begins tracking the given entity, and any other reachable entities that are not
        ///     already being tracked, in the <see cref="Microsoft.EntityFrameworkCore.EntityState.Added"/>
        ///    state such that they will be inserted into the database when <see cref="Save"/>
        ///     is called.
        /// </summary>
        /// <param name="value">The entity to add.</param>
        void Add(object value);
        /// <summary>
        ///  Begins tracking the given entity in the<see cref="Microsoft.EntityFrameworkCore.EntityState.Deleted"/>
        ///  state such that it will be removed from the database when <see cref="Save"/>
        ///  is called.
        /// </summary>
        /// <param name="value"></param>
        void Delete(object value);
        /// <summary>
        /// Finds an entity with the given primary key values. If an entity with the given
        ///     primary key values is being tracked by the context, then it is returned immediately
        ///     without making a request to the database. Otherwise, a query is made to the database
        ///     for an entity with the given primary key values and this entity, if found, is
        ///     attached to the context and returned. If no entity is found, then null is returned.
        /// </summary>
        /// <param name="tableName">Table to search</param>
        /// <param name="keys">The values of the primary key for the entity to be found.</param>
        /// <returns></returns>
        object Find(string tableName, object[] keys);
        /// <summary>
        /// Finds an entity with the given primary key values. If an entity with the given
        ///     primary key values is being tracked by the context, then it is returned immediately
        ///     without making a request to the database. Otherwise, a query is made to the database
        ///     for an entity with the given primary key values and this entity, if found, is
        ///     attached to the context and returned. If no entity is found, then null is returned.
        /// </summary>
        /// <param name="tableName">Table to search</param>
        /// <param name="keys">The values of the primary key for the entity to be found.</param>
        /// <returns>A single value but still as an <see cref="IQueryable"/> to allow for joins.</returns>
        IQueryable<object> FilterByKey(string tableName, object[] keys);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        object GetAll(string tableName);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        object NewInstance(string tableName);
        /// <summary>
        ///  Saves all changes made in this context to the database.
        /// </summary>
        /// <returns> The number of state entries written to the database.</returns>
        int Save();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IDbCommand CreateDbCommand();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableName"></param>
        void SetupDataContext(string tableName);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="outerTableName"></param>
        /// <returns></returns>
        object GetWithoutContext(string tableName, string outerTableName);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        void Update(object value);

        IEnumerable<PrimaryKeyInformation> MapPrimaryKey(object value);
    }
}