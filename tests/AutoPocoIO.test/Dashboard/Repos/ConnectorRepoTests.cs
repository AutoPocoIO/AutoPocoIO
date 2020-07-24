using AutoPocoIO.Context;
using AutoPocoIO.Dashboard.Repos;
using AutoPocoIO.Dashboard.ViewModels;
using AutoPocoIO.Factories;
using AutoPocoIO.Models;
using AutoPocoIO.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;

namespace AutoPocoIO.test.Dashboard.Repos
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class ConnectorRepoTests
    {
        private DbContextOptions<AppDbContext> appDbOptions;
        private IConnectionStringFactory factory;
        private ConnectionInformation connectionInformation = null;

        [TestInitialize]
        public void Init()
        {
            appDbOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "appDb" + Guid.NewGuid().ToString())
                .Options;

            var mockFactory = new Mock<IConnectionStringFactory>();
            mockFactory.Setup(c => c.GetConnectionInformation(1, "conn1"))
                .Returns(new ConnectionInformation
                {
                    InitialCatalog = "cat1",
                    DataSource = "src",
                    UserId = "id"
                });

            mockFactory.Setup(c => c.CreateConnectionString(1, It.IsAny<ConnectionInformation>()))
                .Callback<int, ConnectionInformation>((r, c) => connectionInformation = c)

                /* Unmerged change from project 'AutoPocoIO.test (net461)'
                Before:
                                .Returns("conn1");

                            factory = mockFactory.Object;
                After:
                                .Returns("conn1");

                            factory = mockFactory.Object;
                */
                .Returns("conn1");

            factory = mockFactory.Object;

        }

        [TestMethod]
        public void InsertAddsConnectionToDb()
        {
            var db = new AppDbContext(appDbOptions);
            var repo = new ConnectorRepo(db, factory);

            ConnectorViewModel model = new ConnectorViewModel
            {
                Name = "name1",
                ResourceType = 1,
                RecordLimit = 12,
                Port = 123,
                InitialCatalog = "cat1",
                DataSource = "src",
                UserId = "id"
            };



            string id = repo.Insert(model);

            Connector actual;
            using (var db1 = new AppDbContext(appDbOptions))
            {
                actual = db1.Connector.First(c => c.Name == "name1");
            }

            Assert.AreEqual(1, actual.ResourceType);
            Assert.AreEqual(12, actual.RecordLimit);
            Assert.AreEqual("cat1", connectionInformation.InitialCatalog);
            Assert.AreEqual("src", connectionInformation.DataSource);
            Assert.AreEqual("id", connectionInformation.UserId);
            Assert.AreEqual(123, actual.Port);
            Assert.AreEqual("conn1", actual.ConnectionString);
        }

        [TestMethod]
        public void UpdateConnectionToDb()
        {
            using (var db1 = new AppDbContext(appDbOptions))
            {
                db1.Connector.Add(new Connector
                {
                    Id = "12",
                    Name = "name123"
                });

                db1.SaveChanges();
            }

            var db = new AppDbContext(appDbOptions);
            var repo = new ConnectorRepo(db, factory);

            ConnectorViewModel model = new ConnectorViewModel
            {
                Id = "12",
                Name = "name11",
                ResourceType = 1,
                RecordLimit = 12,
                Port = 123,
                InitialCatalog = "cat1",
                DataSource = "src",
                UserId = "id"
            };

            repo.Save(model);

            Connector actual;
            using (var db1 = new AppDbContext(appDbOptions))
            {
                actual = db1.Connector.First(c => c.Name == "name11");
            }

            Assert.AreEqual(1, actual.ResourceType);
            Assert.AreEqual(12, actual.RecordLimit);
            Assert.AreEqual("cat1", connectionInformation.InitialCatalog);
            Assert.AreEqual("src", connectionInformation.DataSource);
            Assert.AreEqual("id", connectionInformation.UserId);
            Assert.AreEqual(123, actual.Port);
            Assert.AreEqual("conn1", actual.ConnectionString);
        }

        [TestMethod]
        public void GetConnectorById()
        {
            using (var db1 = new AppDbContext(appDbOptions))
            {
                db1.Connector.Add(new Connector
                {
                    Id = "12",
                    Name = "name123",
                    ResourceType = 1,
                    ConnectionString = "conn1"
                });

                db1.SaveChanges();
            }

            var db = new AppDbContext(appDbOptions);
            var repo = new ConnectorRepo(db, factory);

            var actual = repo.GetById("12");

            Assert.AreEqual("name123", actual.Name);
        }

        [TestMethod]
        public void ListConnectorsOrdersByName()
        {
            using (var db1 = new AppDbContext(appDbOptions))
            {
                db1.Connector.AddRange(new Connector
                {
                    Id = "1",
                    Name = "name123",
                },
                new Connector
                {
                    Id = "2",
                    Name = "aname123",
                });

                db1.SaveChanges();
            }

            var db = new AppDbContext(appDbOptions);
            var repo = new ConnectorRepo(db, factory);

            var results = repo.ListConnectors();

            Assert.AreEqual(2, results.Count());
            Assert.AreEqual("aname123", results.First().Name);
        }

        [TestMethod]
        public void DeleteConnectorRemovesFromDb()
        {
            using (var db1 = new AppDbContext(appDbOptions))
            {
                db1.Connector.AddRange(new Connector
                {
                    Id = "1",
                    Name = "name123",
                },
                new Connector
                {
                    Id = "2",
                    Name = "aname123",
                });

                db1.SaveChanges();
            }

            var db = new AppDbContext(appDbOptions);
            var repo = new ConnectorRepo(db, factory);

            repo.Delete("2");

            using (var db1 = new AppDbContext(appDbOptions))
            {
                var results = db1.Connector.ToList();
                Assert.AreEqual(1, results.Count());
                Assert.AreEqual("name123", results.First().Name);
            }
        }

        [TestMethod]
        public void GetConnectorCountFromDb()
        {
            using (var db1 = new AppDbContext(appDbOptions))
            {
                db1.Connector.AddRange(new Connector
                {
                    Id = "1",
                    Name = "name123",
                },
                new Connector
                {
                    Id = "2",
                    Name = "aname123",
                });

                db1.SaveChanges();
            }

            var db = new AppDbContext(appDbOptions);
            var repo = new ConnectorRepo(db, factory);

            var count = repo.ConnectorCount();
            Assert.AreEqual(2, count);
        }




    }
}
