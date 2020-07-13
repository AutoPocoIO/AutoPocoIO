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

                        IEnumerable<Type> types;
                        try
                        {
                            types = assembly.DefinedTypes;
                        }
                        catch(ReflectionTypeLoadException e)
                        {
                            types = e.Types.Where(c => c != null);
                        }

                        var items = types.Where(c => c.IsSubclassOf(typeof(Migration))
                        && c.GetCustomAttribute<DbContextAttribute>()?.ContextType == _contextType)
                            .Select(c => new { c.GetCustomAttribute<MigrationAttribute>()?.Id, Type = c })
                            .OrderBy(c => c.Id);

                        foreach (var item in items)
                        {

                            result.Add(item.Id, item.Type.GetTypeInfo());
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
