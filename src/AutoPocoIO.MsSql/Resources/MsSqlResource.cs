using AutoPocoIO.DynamicSchema.Db;
using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.DynamicSchema.Util;
using AutoPocoIO.Extensions;
using AutoPocoIO.MsSql.DynamicSchema.Db;
using AutoPocoIO.MsSql.DynamicSchema.Runtime;
using AutoPocoIO.MsSql.DynamicSchema.Utils;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace AutoPocoIO.Resources
{
    internal class MsSqlResource : OperationResource, IOperationResource
    {
        public MsSqlResource(IServiceProvider serviceProvider) : base(serviceProvider) { }

        public override ResourceType ResourceType => ResourceType.Mssql;

        public override void ApplyServices(IServiceCollection serviceCollection, IServiceProvider rootProvider)
        {
            serviceCollection.AddTransient<IDbSchemaBuilder, MsSqlDbSchemaBuilder>();
            serviceCollection.AddTransient<MsSqlSchmeaQueries>();
            serviceCollection.AddScoped<IDbTypeMapper, MsSqlDbTypeMapper>();
            serviceCollection.AddTransient<IConnectionStringBuilder, MsSqlConnectionBuilder>();
            base.ApplyServices(serviceCollection, rootProvider);
        }

        public override IDictionary<string, object> ExecuteProc(IDictionary<string, object> parameterDictionary)
        {
            this.LoadProc();


            StoredProcedure spoc = DbSchema.GetStoredProcedure(SchemaName, DbObjectName);

            SqlParameter[] objectParameters = new SqlParameter[spoc.Parameters.Count];
            SqlParameter currentParameter = null;
            DBParameter schemaParameter = null;

            for (int i = 0; i < spoc.Parameters.Count; i++)
            {
                schemaParameter = spoc.Parameters[i];
                if (parameterDictionary.ContainsKey(schemaParameter.Name))
                    currentParameter = new SqlParameter(schemaParameter.Name, parameterDictionary[schemaParameter.Name]);
                else
                    currentParameter = new SqlParameter(schemaParameter.Name, DBNull.Value);

                currentParameter.Direction = schemaParameter.IsOutput ? ParameterDirection.Output : ParameterDirection.Input;
                currentParameter.SqlDbType = (SqlDbType)Enum.Parse(typeof(SqlDbType), schemaParameter.Type, true);
                objectParameters[i] = currentParameter;
            }

            string paramNames = string.Join(",", spoc.Parameters.Select(c => c.Name + (c.IsOutput ? " out" : "")));
            return Db.DynamicListFromSql($"[{spoc.Database}].[{spoc.Schema}].[{spoc.Name}] {paramNames}", objectParameters);
        }
    }
}