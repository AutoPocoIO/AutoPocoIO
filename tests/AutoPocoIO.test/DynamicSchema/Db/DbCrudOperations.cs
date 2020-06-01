using AutoPocoIO.DynamicSchema.Db;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.DynamicSchema.Runtime;
using AutoPocoIO.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoPocoIO.test.DynamicSchema.Db
{
    
    [Trait("Category", TestCategories.Unit)]
    public class DbCrudOperations
    {
        private readonly DbContextOptions<DbContextBase> options;
        private readonly Mock<DbAdapter> db;
        public DbCrudOperations()
        {
            options = new DbContextOptionsBuilder<DbContextBase>()
              .UseInMemoryDatabase(databaseName: "DbCrudOperations_" + Guid.NewGuid().ToString())
              .Options;

            var schemaBuilder = new Mock<IDbSchemaBuilder>().Object;

            var schema = new Mock<IDbSchema>().Object;
            var dbClassBuilder = new Mock<DynamicClassBuilder>(schema).Object;
            db = new Mock<DbAdapter>(schemaBuilder, dbClassBuilder, schema);
        }

        [FactWithName]
        public void InsertRecordAddsToDb()
        {
            using (var dbContextBase = new DbContextBase(options, new Dictionary<string, Type>() { { "tbl", typeof(Connector) } }, new List<Table>()))
            {
                Assert.Equal(0, dbContextBase.Set<Connector>().Count());

                db.SetupGet(c => c.Instance).Returns(dbContextBase);
                db.SetupGet(c => c.DbSetEntityType).Returns(typeof(Connector));

                db.Object.Add(new Connector
                {
                    Name = "test"
                });

                db.Object.Save();
                Assert.Equal(1, dbContextBase.Set<Connector>().Count());
            }
        }

        [FactWithName]
        public void NewReturnsInstance()
        {
            db.Setup(c => c.SetupDataContext("Table"));
            db.SetupGet(c => c.DbSetEntityType).Returns(typeof(Connector));

            var connector = db.Object.NewInstance("Table");

            Assert.IsType<Connector>(connector);
            db.Verify(c => c.SetupDataContext("Table"), Times.Once);
        }

        [FactWithName]
        public void GetReturnsDbSet()
        {
            using (var dbContextBase = new DbContextBase(options, new Dictionary<string, Type>() { { "tbl", typeof(Connector) } }, new List<Table>()))
            {
                db.Setup(c => c.SetupDataContext("Table"));
                db.SetupGet(c => c.DbSetEntityType).Returns(typeof(Connector));
                db.SetupGet(c => c.Instance).Returns(dbContextBase);

                var connectorSet = db.Object.GetAll("Table");

                Assert.IsAssignableFrom<DbSet<Connector>>(connectorSet);
                db.Verify(c => c.SetupDataContext("Table"), Times.Once);
            }
        }

        [FactWithName]
        public void FindRecord()
        {
            using (var dbContextBase = new DbContextBase(options, new Dictionary<string, Type>() { { "tbl", typeof(Connector) } }, new List<Table>()))
            {
                db.Setup(c => c.SetupDataContext("Table"));
                db.SetupGet(c => c.Instance).Returns(dbContextBase);
                db.SetupGet(c => c.DbSetEntityType).Returns(typeof(Connector));

                dbContextBase.Add(new Connector
                {
                    Id = 1,
                    Name = "init",
                    Port = 123
                });
                dbContextBase.SaveChanges();
                Assert.Equal(1, dbContextBase.Set<Connector>().Count());

                var connector = db.Object.Find("Table", "1");
                Assert.Equal("init", ((Connector)connector).Name);
                Assert.Equal(123, ((Connector)connector).Port);
                db.Verify(c => c.SetupDataContext("Table"), Times.Once);
            }
        }

        [FactWithName]
        public void UpdateRecordUpdatesToDb()
        {
            using (var dbContextBase = new DbContextBase(options, new Dictionary<string, Type>() { { "tbl", typeof(Connector) } }, new List<Table>()))
            {
                dbContextBase.Add(new Connector
                {
                    Name = "init"
                });
                dbContextBase.SaveChanges();
                Assert.Equal(1, dbContextBase.Set<Connector>().Count());

                db.SetupGet(c => c.Instance).Returns(dbContextBase);
                db.SetupGet(c => c.DbSetEntityType).Returns(typeof(Connector));

                var connector = dbContextBase.Set<Connector>().First();
                connector.Name = "updated";

                db.Object.Update(connector);
                db.Object.Save();
                Assert.Equal("updated", dbContextBase.Set<Connector>().First().Name);
            }
        }

        [FactWithName]
        public void RemoveRecordUpdatesToDb()
        {
            using (var dbContextBase = new DbContextBase(options, new Dictionary<string, Type>() { { "tbl", typeof(Connector) } }, new List<Table>()))
            {
                dbContextBase.Add(new Connector
                {
                    Name = "init"
                });
                dbContextBase.SaveChanges();
                Assert.Equal(1, dbContextBase.Set<Connector>().Count());

                db.SetupGet(c => c.Instance).Returns(dbContextBase);
                db.SetupGet(c => c.DbSetEntityType).Returns(typeof(Connector));

                var connector = dbContextBase.Set<Connector>().First();
                db.Object.Delete(connector);
                db.Object.Save();

                Assert.Equal(0, dbContextBase.Set<Connector>().Count());
            }
        }
    }
}