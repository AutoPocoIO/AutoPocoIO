using AutoPocoIO.EntityConfiguration;
using AutoPocoIO.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoPocoIO.Extensions
{
    internal static class ModelBuilderExtensions
    {
        public static void CreateModel(this ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ConnectorConfiguration());
            modelBuilder.ApplyConfiguration(new EntityConfiguration.UserJoinEntityConfiguration());

        }

        public static void Seed(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Connector>()
                .HasData(new Connector
                {
                    Id = "4b6b6ba7-0209-4b89-91cb-0e2a67aa37c1",
                    Name = AutoPocoConstants.DefaultConnectors.AppDB,
                    ResourceType = "",
                    ConnectionString = "",
                    Schema = "AutoPoco",
                    RecordLimit = 500,

                },
                new Connector
                {
                    Id = "4d74e770-54e9-4b0f-8f13-59ccb0808654",
                    Name = AutoPocoConstants.DefaultConnectors.Logging,
                    ResourceType = "",
                    ConnectionString = "",
                    Schema = "AutoPocoLog",
                    RecordLimit = 500
                });

            modelBuilder.Entity<UserJoin>().HasData(
                new UserJoin
                {
                    Id = "abd7f037-cc34-44fb-89f5-2e4a06772a01",
                    Alias = "Response",
                    PKConnectorId = "4d74e770-54e9-4b0f-8f13-59ccb0808654",
                    FKConnectorId = "4d74e770-54e9-4b0f-8f13-59ccb0808654",
                    PKTableName = "Request",
                    PKColumn = "RequestId,RequestGuid",
                    FKTableName = "Response",
                    FKColumn = "ResponseId,RequestGuid"
                });
        }


    }
}
