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
            modelBuilder.ApplyConfiguration(new EntityConfiguration.UserJoinConfiguration());

        }

        public static void Seed(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Connector>()
                .HasData(new Connector
                {
                    Id = 1,
                    Name = AutoPocoConstants.DefaultConnectors.AppDB,
                    ResourceType = 1,
                    ConnectionString = "",
                    Schema = "AutoPoco",
                    RecordLimit = 500,

                },
                new Connector
                {
                    Id = 2,
                    Name = AutoPocoConstants.DefaultConnectors.Logging,
                    ResourceType = 1,
                    ConnectionString = "",
                    Schema = "AutoPocoLog",
                    RecordLimit = 500
                });

            modelBuilder.Entity<UserJoin>().HasData(
                new UserJoin
                {
                    Id = 1,
                    Alias = "Response",
                    PKConnectorId = 2,
                    FKConnectorId = 2,
                    PKTableName = "Request",
                    PKColumn = "RequestId,RequestGuid",
                    FKTableName = "Response",
                    FKColumn = "ResponseId,RequestGuid"
                });
        }

        
    }
}
