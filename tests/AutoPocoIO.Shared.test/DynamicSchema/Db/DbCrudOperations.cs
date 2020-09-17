using AutoPocoIO.DynamicSchema.Db;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.DynamicSchema.Runtime;
using AutoPocoIO.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoPocoIO.test.DynamicSchema.Db
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class DbCrudOperations
    {
        private DbContextOptions<DbContextBase> options;
        private Mock<DbAdapter> db;

        [TestInitialize]
        public void Init()
        {
            options = new DbContextOptionsBuilder<DbContextBase>()
              .UseInMemoryDatabase(databaseName: "DbCrudOperations_" + Guid.NewGuid().ToString())
              .Options;

            var schemaBuilder = new Mock<IDbSchemaBuilder>().Object;

            var schema = new Mock<IDbSchema>().Object;
            var dbClassBuilder = new Mock<DynamicClassBuilder>(schema).Object;
            db = new Mock<DbAdapter>(schemaBuilder, dbClassBuilder, schema);
        }

        [TestMethod]
        public void InsertRecordAddsToDb()
        {
            using (var dbContextBase = new DbContextBase(options, new Dictionary<string, Type>() { { "tbl", typeof(Connector) } }, new List<Table>()))
            {
                Assert.AreEqual(0, dbContextBase.Set<Connector>().Count(), "Inital blank state");

                db.SetupGet(c => c.Instance).Returns(dbContextBase);
                db.SetupGet(c => c.DbSetEntityType).Returns(typeof(Connector));

                db.Object.Add(new Connector
                {
                    Name = "test"
                });

                db.Object.Save();
                Assert.AreEqual(1, dbContextBase.Set<Connector>().Count(), "Insert failed");
            }
        }

        [TestMethod]
        public void NewReturnsInstance()
        {
            db.Setup(c => c.SetupDataContext("Table"));
            db.SetupGet(c => c.DbSetEntityType).Returns(typeof(Connector));

            var connector = db.Object.NewInstance("Table");

            Assert.IsInstanceOfType(connector, typeof(Connector));
            db.Verify(c => c.SetupDataContext("Table"), Times.Once);
        }

        [TestMethod]
        public void GetReturnsDbSet()
        {
            using (var dbContextBase = new DbContextBase(options, new Dictionary<string, Type>() { { "tbl", typeof(Connector) } }, new List<Table>()))
            {
                db.Setup(c => c.SetupDataContext("Table"));
                db.SetupGet(c => c.DbSetEntityType).Returns(typeof(Connector));
                db.SetupGet(c => c.Instance).Returns(dbContextBase);

                var connectorSet = db.Object.GetAll("Table");

                Assert.IsInstanceOfType(connectorSet, typeof(DbSet<Connector>));
                db.Verify(c => c.SetupDataContext("Table"), Times.Once);
            }
        }

        [TestMethod]
        public void FindRecord()
        {
            using (var dbContextBase = new DbContextBase(options, new Dictionary<string, Type>() { { "tbl", typeof(Connector) } }, new List<Table>()))
            {
                db.Setup(c => c.SetupDataContext("Table"));
                db.SetupGet(c => c.Instance).Returns(dbContextBase);
                db.SetupGet(c => c.DbSetEntityType).Returns(typeof(Connector));

                dbContextBase.Add(new Connector
                {
                    Id = "1",
                    Name = "init",
                    Port = 123
                });
                dbContextBase.SaveChanges();
                Assert.AreEqual(1, dbContextBase.Set<Connector>().Count(), "Inital state invalid");

                var connector = db.Object.Find("Table", new object[] { "1" });
                Assert.AreEqual("init", ((Connector)connector).Name);
                Assert.AreEqual(123, ((Connector)connector).Port);
                db.Verify(c => c.SetupDataContext("Table"), Times.Once);
            }
        }

        [TestMethod]
        public void UpdateRecordUpdatesToDb()
        {
            using (var dbContextBase = new DbContextBase(options, new Dictionary<string, Type>() { { "tbl", typeof(Connector) } }, new List<Table>()))
            {
                dbContextBase.Add(new Connector
                {
                    Name = "init"
                });
                dbContextBase.SaveChanges();
                Assert.AreEqual(1, dbContextBase.Set<Connector>().Count(), "Inital state invalid");

                db.SetupGet(c => c.Instance).Returns(dbContextBase);
                db.SetupGet(c => c.DbSetEntityType).Returns(typeof(Connector));

                var connector = dbContextBase.Set<Connector>().First();
                connector.Name = "updated";

                db.Object.Update(connector);
                db.Object.Save();
                Assert.AreEqual("updated", dbContextBase.Set<Connector>().First().Name);
            }
        }

        [TestMethod]
        public void RemoveRecordUpdatesToDb()
        {
            using (var dbContextBase = new DbContextBase(options, new Dictionary<string, Type>() { { "tbl", typeof(Connector) } }, new List<Table>()))
            {
                dbContextBase.Add(new Connector
                {
                    Name = "init"
                });
                dbContextBase.SaveChanges();
                Assert.AreEqual(1, dbContextBase.Set<Connector>().Count(), "Inital state invalid");

                db.SetupGet(c => c.Instance).Returns(dbContextBase);
                db.SetupGet(c => c.DbSetEntityType).Returns(typeof(Connector));

                var connector = dbContextBase.Set<Connector>().First();
                db.Object.Delete(connector);
                db.Object.Save();

                Assert.AreEqual(0, dbContextBase.Set<Connector>().Count());
            }
        }
    }
}