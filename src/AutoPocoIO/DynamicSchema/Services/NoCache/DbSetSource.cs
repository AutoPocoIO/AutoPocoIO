using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace AutoPocoIO.DynamicSchema.Services.NoCache
{
    [ExcludeFromCodeCoverage]
    internal class DbSetSource : Microsoft.EntityFrameworkCore.Internal.DbSetSource
    {
        private static readonly MethodInfo _genericCreateSet
           = typeof(DbSetSource).GetTypeInfo().GetDeclaredMethod(nameof(CreateSetFactory));

      

        public override object Create(DbContext context, Type type)
        {
            return CreateCore(context, type, _genericCreateSet);
        }

#if NETCORE2_2 || NETFULL
        private static readonly MethodInfo _genericCreateQuery
            = typeof(DbSetSource).GetTypeInfo().GetDeclaredMethod(nameof(CreateQueryFactory));

        public override object CreateQuery(DbContext context, Type type)
        {
            return CreateCore(context, type, _genericCreateQuery);
        }

            private static Func<DbContext, DbQuery<TQuery>> CreateQueryFactory<TQuery>()
            where TQuery : class
            => c => new InternalDbQuery<TQuery>(c);
#endif

        private object CreateCore(DbContext context, Type type, MethodInfo createMethod)
        {
            var func = (Func<DbContext, object>)createMethod
                  .MakeGenericMethod(type)
                  .Invoke(null, null);

            return func(context);
        }

        private static Func<DbContext, object> CreateSetFactory<TEntity>()
           where TEntity : class
           => c => new InternalDbSet<TEntity>(c);

    
    }
}