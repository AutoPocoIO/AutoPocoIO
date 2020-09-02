using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.Resources;
using System.Data.SqlClient;

namespace AutoPocoIO.MsSql.DynamicSchema.Db
{
    public class MsSqlConnectionBuilder : IConnectionStringBuilder
    {
        public string ResourceType => typeof(Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal.SqlServerOptionsExtension).Assembly.GetName().Name;

        public string CreateConnectionString(ConnectionInformation connectionInformation)
        {
            SqlConnectionStringBuilder msSqlbuilder = new SqlConnectionStringBuilder
            {
                InitialCatalog = connectionInformation.InitialCatalog,
                UserID = connectionInformation.UserId,
                DataSource = connectionInformation.DataSource,
                Password = connectionInformation.Password,
                PersistSecurityInfo = false,
                MultipleActiveResultSets = false,
                TrustServerCertificate = false,
                ConnectTimeout = 30
            };

            return msSqlbuilder.ConnectionString;
        }

        public ConnectionInformation ParseConnectionString(string connectionString)
        {
            var msSqlbuilder = new SqlConnectionStringBuilder(connectionString);
            return new ConnectionInformation
            {
                InitialCatalog = msSqlbuilder.InitialCatalog,
                UserId = msSqlbuilder.UserID,
                DataSource = msSqlbuilder.DataSource,
                Password = msSqlbuilder.Password,
                ResourceType = this.ResourceType
            };
        }
    }
}
