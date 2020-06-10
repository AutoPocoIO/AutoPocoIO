using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AutoPocoIO.DynamicSchema.Services.Migrations
{
    internal class MigrationsAssembly : Microsoft.EntityFrameworkCore.Migrations.Internal.MigrationsAssembly
    {
        private readonly Assembly[] _autoPocoAssemblies;
        private IReadOnlyDictionary<string, TypeInfo> _migrations;
        private readonly Type _contextType;
        public MigrationsAssembly(ICurrentDbContext currentContext,
                                  IDbContextOptions options,
                                  IMigrationsIdGenerator idGenerator,
                                  IDiagnosticsLogger<DbLoggerCategory.Migrations> logger) : base(currentContext, options, idGenerator, logger)
        {

            _contextType = currentContext.Context.GetType();
            _autoPocoAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => x.FullName.ToUpperInvariant().Contains("AUTOPOCOIO"))
                .ToArray();
        }

        public override IReadOnlyDictionary<string, TypeInfo> Migrations
        {
            get
            {
                IReadOnlyDictionary<string, TypeInfo> Create()
                {
                    var result = new SortedList<string, TypeInfo>();

                    foreach (var assembly in _autoPocoAssemblies)
                    {
                        var items
                            = from t in assembly.DefinedTypes
                              where t.IsSubclassOf(typeof(Migration))
                            && t.GetCustomAttribute<DbContextAttribute>()?.ContextType == _contextType
                              let id = t.GetCustomAttribute<MigrationAttribute>()?.Id
                              orderby id
                              select (id, t);

                        foreach (var (id, t) in items)
                        {

                            result.Add(id, t);
                        }
                    }
                    

                    return result;
                }

                if (_migrations == null)
                    _migrations = Create();

                return _migrations;
            }
        }


    }
}
