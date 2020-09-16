using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Threading;

namespace AutoPocoIO.DynamicSchema.Services.NoCache
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal class ModelSource : Microsoft.EntityFrameworkCore.Infrastructure.ModelSource
    {
        public ModelSource(ModelSourceDependencies dependencies) : base(dependencies)
        { }



        public override IModel GetModel(DbContext context, Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure.IConventionSetBuilder conventionSetBuilder)
        {
            return new Lazy<IModel>(
                     () => CreateModel(context, conventionSetBuilder),
                     LazyThreadSafetyMode.ExecutionAndPublication).Value;
        }

    }
}
