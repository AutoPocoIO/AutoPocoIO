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
        public override IModel GetModel(DbContext context, Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal.IConventionSetBuilder conventionSetBuilder, IModelValidator validator)
        {
            return new Lazy<IModel>(
                     () => CreateModel(context, conventionSetBuilder, validator),
                     LazyThreadSafetyMode.ExecutionAndPublication).Value;
        }

    }
}
