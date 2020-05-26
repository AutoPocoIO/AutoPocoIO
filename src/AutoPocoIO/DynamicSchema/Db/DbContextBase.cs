using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.Exceptions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace AutoPocoIO.DynamicSchema.Db
{
    /// <summary>
    /// Dynamic schema context
    /// </summary>
    public class DbContextBase : DbContext, IDbContextBase
    {
        private readonly Dictionary<string, Type> _assemblyTypes;
        private readonly List<Table> _tables;

        internal DbContextBase(DbContextOptions options, Dictionary<string, Type> assemblyTypes, List<Table> tables)
        : base(options)
        {
            _assemblyTypes = assemblyTypes;
            _tables = tables;
        }

        /// <summary>
        /// Set up Virtual Entity relationships, Compund PKs and create Entity methods
        /// </summary>
        /// <param name="modelBuilder">Builder to add relationships to in this context</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));

            var entityMethod = typeof(ModelBuilder).GetMethod("Entity", Array.Empty<Type>());
            foreach (var type in _assemblyTypes)
            {
                entityMethod.MakeGenericMethod(type.Value).Invoke(modelBuilder, Array.Empty<object>());
            }

            foreach (var table in _tables)
            {
                var pks = table.Columns.Where(c => c.IsPK)
                                       .OrderBy(c => c.PKPosition)
                                        .Select(c => c.ColumnName);

                var tableType = GetTypeFromAssemblies(table.VariableName);

                if (pks.Any())
                    modelBuilder.Entity(tableType).HasKey(pks.ToArray());
                else
                    throw new NoPrimaryKeyFoundException($"{table.Database}.{table.Schema}.{table.Name}");    
            }
        }

        public virtual IDbCommand CreateDbCommand()
        {
            return Database.GetDbConnection().CreateCommand();
        }

        private Type GetTypeFromAssemblies(string variableName)
        {
            return _assemblyTypes.First(c => c.Value.Name.ToUpperInvariant() == variableName.ToUpperInvariant()).Value;
        }
    }
}