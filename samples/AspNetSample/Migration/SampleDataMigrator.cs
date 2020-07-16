using AutoPocoIO.Api;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace AspNetCoreSample.Migration
{
    public class SampleDataMigrator
    {
        private readonly string _connection;

        public SampleDataMigrator(string connection)
        {
            _connection = connection;
        }

        public void Migrate()
        {
            try
            {
                Assembly asm = this.GetType().Assembly;
                using (var connection = new SqlConnection(_connection))
                {
                    connection.Open();
                    //Create objects
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = GetResource(asm, "Create");
                        cmd.ExecuteNonQuery();
                    }

                    //Seed data
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = GetResource(asm, "LoadData");
                        cmd.ExecuteNonQuery();
                    }
                }

               
            }
            catch (Exception) { }

            //Try Seed sampleDb connection
            var tableOp = DependencyResolver.Current.GetService<ITableOperations>();
            var builder = new SqlConnectionStringBuilder(_connection);

            var connectionEntity = new
            {
                Id = Guid.NewGuid().ToString(),
                Name = "sampleSales",
                ResourceType = 1,
                Schema = "sales",
                ConnectionString = _connection,
                RecordLimit = 500,
                builder.InitialCatalog,
                builder.DataSource,
                builder.UserID,
                IsActive = true
            };

            try
            {
                tableOp.CreateNewRow("appDb", "connector", connectionEntity);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException) { }
        }

        private static string GetResource(Assembly assembly, string sqlName)
        {
            using (Stream s = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.Migration.{sqlName}.sql"))
            using(StreamReader reader = new StreamReader(s))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
