using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AspNetCoreSample.Migration
{
    public class Migrator
    {
        private readonly string _connection;

        public Migrator(string connection)
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
