using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace AutoPocoIO.DynamicSchema.Services.NoCache
{
    [ExcludeFromCodeCoverage]
    internal class EntityFinderSource : Microsoft.EntityFrameworkCore.Internal.EntityFinderSource
    {
        private static readonly MethodInfo _genericCreate
          = typeof(EntityFinderSource).GetTypeInfo().GetDeclaredMethod(nameof(CreateConstructor));

        public override IEntityFinder Create(IStateManager stateManager, IDbSetSource setSource, IDbSetCache setCache, IEntityType type)
        {
            var func = (Func<IStateManager, IDbSetSource, IDbSetCache, IEntityType, IEntityFinder>)
                    _genericCreate.MakeGenericMethod(type.ClrType).Invoke(null, null);

            return func(stateManager, setSource, setCache, type);
        }

        private static Func<IStateManager, IDbSetSource, IDbSetCache, IEntityType, IEntityFinder> CreateConstructor<TEntity>()
           where TEntity : class
           => (s, src, c, t) => new EntityFinder<TEntity>(s, src, c, t);

    }
}