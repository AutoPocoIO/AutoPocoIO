
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace AutoPocoIO.DynamicSchema.Services.NoCache
{
    [ExcludeFromCodeCoverage]
    internal class CompiledQueryCache : Microsoft.EntityFrameworkCore.Query.Internal.CompiledQueryCache
    {
        public CompiledQueryCache(IMemoryCache memoryCache) : base(memoryCache)
        { }


        public override Func<QueryContext, IAsyncEnumerable<TResult>> GetOrAddAsyncQuery<TResult>(object cacheKey, Func<Func<QueryContext, IAsyncEnumerable<TResult>>> compiler)
        {
            return GetOrAddQueryCore(compiler);
        }

        public override Func<QueryContext, TResult> GetOrAddQuery<TResult>(object cacheKey, Func<Func<QueryContext, TResult>> compiler)
        {
            return GetOrAddQueryCore(compiler);
        }

        private Func<QueryContext, TFunc> GetOrAddQueryCore<TFunc>(
            Func<Func<QueryContext, TFunc>> compiler)
        {
            return compiler();
        }
    }
}