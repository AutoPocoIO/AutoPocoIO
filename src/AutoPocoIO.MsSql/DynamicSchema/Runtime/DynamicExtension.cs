using AutoPocoIO.DynamicSchema.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Globalization;

namespace AutoPocoIO.MsSql.DynamicSchema.Runtime
{
    internal static class DynamicExtension
    {
        public static DbContextOptionsBuilder ReplaceSqlServerEFCachingServices(this DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ReplaceService<IRelationalTypeMappingSource, AutoPocoIO.DynamicSchema.Services.NoCache.SqlServerTypeMappingSource>();
            return optionsBuilder;
        }

        public static DbContextOptionsBuilder ReplaceSqlServerEFCrossDbServices(this DbContextOptionsBuilder optionBuilder)
        {
            optionBuilder.ReplaceService<IQuerySqlGeneratorFactory, AutoPocoIO.DynamicSchema.Services.CrossDb.SqlServerQuerySqlGeneratorFactory>();
            optionBuilder.ReplaceService<IModelValidator, AutoPocoIO.DynamicSchema.Services.CrossDb.SqlServerModelValidator>();
            return optionBuilder;

        }

        public static IDictionary<string, object> DynamicListFromSql(this IDbContextBase db, string Sql, params DbParameter[] Params)
        {
            using (var cmd = db.CreateDbCommand())
            {
                cmd.CommandText = Sql;
                if (cmd.Connection.State != ConnectionState.Open)
                    cmd.Connection.Open();

                IDictionary<string, object> outputParams = new Dictionary<string, object>();

                if (Params != null)
                {
                    foreach (var param in Params)
                    {
                        cmd.Parameters.Add(param);
                        if (param.Direction != ParameterDirection.Input)
                            outputParams.Add(param.ParameterName, param.Value);
                    }
                }

                using (var dataReader = cmd.ExecuteReader())
                {
                    int resultCount = 0;
                    do
                    {
                        List<IDictionary<string, object>> Results = new List<IDictionary<string, object>>();

                        while (dataReader.Read())
                        {
                            var row = new ExpandoObject() as IDictionary<string, object>;
                            for (var fieldCount = 0; fieldCount < dataReader.FieldCount; fieldCount++)
                            {
                                string columnName = string.IsNullOrWhiteSpace(dataReader.GetName(fieldCount)) ? "Column" + fieldCount :
                                    dataReader.GetName(fieldCount);

                                row.Add(columnName, dataReader[fieldCount]);
                            }

                            Results.Add(row);
                        }

                        outputParams.Add("ResultSet" + (resultCount == 0 ? "" : resultCount.ToString(CultureInfo.InvariantCulture)), Results);
                        resultCount++;

                    } while (dataReader.NextResult());
                }

                return outputParams;
            }
        }
    }
}
