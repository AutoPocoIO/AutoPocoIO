using AutoPocoIO.Context;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.Extensions;
using AutoPocoIO.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Xunit;

namespace AutoPocoIO.test.Extensions
{
    
    [Trait("Category", TestCategories.Unit)]
    public class ResourceExtensionTests
    {
        readonly DbContextOptions<AppDbContext> appDbOptionsPro = new DbContextOptionsBuilder<AppDbContext>()
              .UseInMemoryDatabase(databaseName: "appDb" + Guid.NewGuid().ToString())
              .Options;

        public ResourceExtensionTests()
        {
            using (var db = new AppDbContext(appDbOptionsPro))
            {
                db.Connector.AddRange(new Connector
                {
                    Id = 1,
                    ConnectionStringDecrypted = "conn1",
                },
                new Connector
                {
                    Id = 2,
                    ConnectionStringDecrypted = "conn2",
                });
                db.UserJoin.Add(new UserJoin
                {
                    PKTableName = "pk",
                    PKConnectorId = 1,
                    FKTableName = "fk",
                    FKConnectorId = 2,
                });
                db.SaveChanges();
            }
        }

        [FactWithName]
        public void UnionUsedConnectorWithJoinsPkMatch()
        {
            var config = new Config { IncludedTable = "pk" };
            config.LoadUserDefinedTables(new Connector { Id = 1, ConnectionStringDecrypted = "newConn" }, new AppDbContext(appDbOptionsPro));

             Assert.Equal(new[] { "newConn", "conn1", "conn2" }, config.UsedConnectors.ToList());
        }

        [FactWithName]
        public void UnionUsedConnectorWithJoinsFkMatch()
        {
            var config = new Config { IncludedTable = "fk" };
            config.LoadUserDefinedTables(new Connector { Id = 2, ConnectionStringDecrypted = "newConn" }, new AppDbContext(appDbOptionsPro));

             Assert.Equal(new[] { "newConn", "conn1", "conn2" }, config.UsedConnectors.ToList());
        }

        [FactWithName]
        public void UnionUsedConnectorWithDistinct()
        {
            var config = new Config { IncludedTable = "fk" };
            config.LoadUserDefinedTables(new Connector { Id = 2, ConnectionStringDecrypted = "conn1" }, new AppDbContext(appDbOptionsPro));

             Assert.Equal(new[] { "conn1", "conn2" }, config.UsedConnectors.ToList());
        }

        [FactWithName]
        public void UnionUsedConnectorWithJoinsNoMatch()
        {
            var config = new Config { IncludedTable = "fk" };
            config.LoadUserDefinedTables(new Connector { Id = 3, ConnectionStringDecrypted = "newConn" }, new AppDbContext(appDbOptionsPro));

             Assert.Equal(new[] { "newConn" }, config.UsedConnectors.ToList());
        }
    }
}
