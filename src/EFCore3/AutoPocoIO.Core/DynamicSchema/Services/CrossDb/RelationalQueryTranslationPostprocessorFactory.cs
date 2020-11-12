using Microsoft.EntityFrameworkCore.Query;

namespace AutoPocoIO.DynamicSchema.Services.CrossDb
{
    internal class RelationalQueryTranslationPostprocessorFactory : Microsoft.EntityFrameworkCore.Query.Internal.RelationalQueryTranslationPostprocessorFactory
    {
        private readonly QueryTranslationPostprocessorDependencies _dependencies;
        private readonly RelationalQueryTranslationPostprocessorDependencies _relationalDependencies;

        public RelationalQueryTranslationPostprocessorFactory(
            QueryTranslationPostprocessorDependencies dependencies,
            RelationalQueryTranslationPostprocessorDependencies relationalDependencies)
            :base(dependencies, relationalDependencies)
        {
            _dependencies = dependencies;
            _relationalDependencies = relationalDependencies;
        }

        public override QueryTranslationPostprocessor Create(QueryCompilationContext queryCompilationContext)
        {
            return new RelationalQueryTranslationPostprocessor(
                _dependencies,
                _relationalDependencies,
                queryCompilationContext);
        }
    }
}
