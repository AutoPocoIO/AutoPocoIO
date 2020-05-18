using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace AutoPocoIO.Resources
{
    public interface IOperationResource : IDynamicResource
    {

        void ConfigureAction(Connector connector, OperationType dbAction, string dbObjectName);

        TViewModel CreateNewResourceRecord<TViewModel>(TViewModel value);
        object CreateNewResourceRecord(JToken value);
        object DeleteResourceRecordById(string keys);
        object ExecuteProc(IDictionary<string, object> parameterDictionary);

        object GetResourceRecordById(string keys);
        IQueryable<object> GetResourceRecords(IDictionary<string, string> queryString);
        IQueryable<object> GetViewRecords();

        void LoadDbAdapter();
        object UpdateResourceRecordById(JToken value, string keys);
        TViewModel UpdateResourceRecordById<TViewModel>(TViewModel value, string keys) where TViewModel : class;

        ColumnDefinition GetColumnDefinition(string columnName);
        SchemaDefinition GetSchemaDefinition();
        StoredProcedureDefinition GetStoredProcedureDefinition();
        StoredProcedureParameterDefinition GetStoredProcedureDefinition(string parameterName);
        TableDefinition GetTableDefinition();

        void LoadSchema(bool includeVirtualEntities);
        void LoadProc(string schemaName, string storedProcedureName);
    }
}