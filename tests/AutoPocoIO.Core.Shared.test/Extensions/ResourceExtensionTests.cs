using AutoPocoIO.Context;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.Extensions;
using AutoPocoIO.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace AutoPocoIO.test.Extensions
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class ResourceExtensionTests
    {
        readonly DbContextOptions<AppDbContext> appDbOptionsPro = new DbContextOptionsBuilder<AppDbContext>()
              .UseInMemoryDatabase(databaseName: "appDb" + Guid.NewGuid().ToString())
              .Options;
        Guid id = Guid.NewGuid();
        Guid id2 = Guid.NewGuid();
        [TestInitialize]
        public void Init()
        {

            using (AppDbContext db = new AppDbContext(appDbOptionsPro))
            {
                db.Connector.AddRange(new Connector
                {
                    Id = id,
                    ConnectionStringDecrypted = "conn1",
                },
                new Connector
                {
                    Id = id2,
                    ConnectionStringDecrypted = "conn2",
                });
                db.UserJoin.Add(new UserJoin
                {
                    Id = id,
                    PKTableName = "pk",
                    PKConnectorId = id,
                    FKTableName = "fk",
                    FKConnectorId = id2,
                });
                db.SaveChanges();
            }
        }

        [TestMethod]
        public void UnionUsedConnectorWithJoinsPkMatch()
        {
            var config = new Config { IncludedTable = "pk" };
            config.LoadUserDefinedTables(new Connector { Id = id, ConnectionStringDecrypted = "newConn" }, new AppDbContext(appDbOptionsPro));

            CollectionAssert.AreEqual(new[] { "newConn", "conn1", "conn2" }, config.UsedConnectors.ToList());
        }

        [TestMethod]
        public void UnionUsedConnectorWithJoinsFkMatch()
        {
            var config = new Config { IncludedTable = "fk" };
            config.LoadUserDefinedTables(new Connector { Id = id2, ConnectionStringDecrypted = "newConn" }, new AppDbContext(appDbOptionsPro));

            CollectionAssert.AreEqual(new[] { "newConn", "conn1", "conn2" }, config.UsedConnectors.ToList());
        }

        [TestMethod]
        public void UnionUsedConnectorWithDistinct()
        {
            var config = new Config { IncludedTable = "fk" };
            config.LoadUserDefinedTables(new Connector { Id = id2, ConnectionStringDecrypted = "conn1" }, new AppDbContext(appDbOptionsPro));

            CollectionAssert.AreEqual(new[] { "conn1", "conn2" }, config.UsedConnectors.ToList());
        }

        [TestMethod]
        public void UnionUsedConnectorWithJoinsNoMatch()
        {
            var config = new Config { IncludedTable = "fk" };
            config.LoadUserDefinedTables(new Connector { Id = Guid.NewGuid(), ConnectionStringDecrypted = "newConn" }, new AppDbContext(appDbOptionsPro));

            CollectionAssert.AreEqual(new[] { "newConn" }, config.UsedConnectors.ToList());
        }
    }
}
