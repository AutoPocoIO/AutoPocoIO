using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.Models;

namespace AutoPocoIO.DynamicSchema.Db
{
    internal class SchemaInitializer : ISchemaInitializer
    {
        private readonly Config _config;
        private readonly IDbSchemaBuilder _schemaBuilder;
        public SchemaInitializer(Config config,
                                 IDbSchemaBuilder schemaBuilder)
        {
            _config = config;
            _schemaBuilder = schemaBuilder;
        }

        public void ConfigureAction(Connector connector, OperationType dbAction)
        {
            _config.ConnectionString = connector.ConnectionStringDecrypted;
        }

        public void Initilize()
        {
            if (!string.IsNullOrEmpty(_config.IncludedTable))
                _schemaBuilder.GetColumns();
            else
            {
                _schemaBuilder.GetTableViews();
                _schemaBuilder.GetStoredProcedures();
            }
        }
    }
}
