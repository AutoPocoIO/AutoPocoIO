using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.Models;

namespace AutoPocoIO.DynamicSchema.Db
{
    internal class SchemaInitializer : ISchemaInitializer
    {
        private readonly Config _config;
        private readonly IDbSchemaBuilder _schemaBuilder;
        private readonly IDbSchema _dbSchema;
        public SchemaInitializer(Config config,
                                 IDbSchemaBuilder schemaBuilder,
                                 IDbSchema dbSchema)
        {
            _config = config;
            _schemaBuilder = schemaBuilder;
            _dbSchema = dbSchema;
        }

        public void ConfigureAction(Connector connector, OperationType dbAction)
        {
            _config.ConnectionString = connector.ConnectionStringDecrypted;
        }

        public void FindSchemas()
        {
            _dbSchema.Reset();
            _schemaBuilder.GetSchemas();
        }

        public void Initilize()
        {
            _dbSchema.Reset();

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
