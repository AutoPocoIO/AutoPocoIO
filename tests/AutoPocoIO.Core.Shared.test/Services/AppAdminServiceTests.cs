using AutoPocoIO.Context;
using AutoPocoIO.EntityConfiguration;
using AutoPocoIO.Exceptions;
using AutoPocoIO.Models;
using AutoPocoIO.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace AutoPocoIO.test.Services
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class AppAdminServiceTests
    {
        DbContextOptions<AppDbContext> appDbOptions;

        [TestInitialize]
        public void Init()
        {
            appDbOptions = new DbContextOptionsBuilder<AppDbContext>()
              .UseInMemoryDatabase(databaseName: "appDb" + Guid.NewGuid().ToString())
              .Options;
        }

        [TestMethod]
        public void FindConnector()
        {
            var db = new AppDbContext(appDbOptions, new ContextEntityConfiguration());
            db.Connector.Add(new Connector
            {
                Id = "1",
                Name = "connName1",
                IsActive = true
            });
            db.SaveChanges();

            IAppAdminService appAdminService = new AppAdminService(db);

            var connector = appAdminService.GetConnection("connName1");

            Assert.AreEqual("1", connector.Id);
            Assert.AreEqual("connName1", connector.Name);
        }


        [TestMethod]
        [ExpectedException(typeof(ConnectorNotFoundException))]
        public void ConnectorNotFound()
        {

            var db = new AppDbContext(appDbOptions, new ContextEntityConfiguration());
            db.Connector.Add(new Connector
            {
                Name = "connName1"
            });
            db.SaveChanges();

            IAppAdminService appAdminService = new AppAdminService(db);
            _ = appAdminService.GetConnection("connName123");
        }

        [TestMethod]
        [ExpectedException(typeof(ConnectorNotFoundException))]
        public void ConnectorNotActive()
        {

            var db = new AppDbContext(appDbOptions, new ContextEntityConfiguration());
            db.Connector.Add(new Connector
            {
                Name = "connName1",
                IsActive = false
            });
            db.SaveChanges();

            IAppAdminService appAdminService = new AppAdminService(db);
            _ = appAdminService.GetConnection("connName1");
        }

        [TestMethod]
        public void FindConnectorById()
        {
            var db = new AppDbContext(appDbOptions, new ContextEntityConfiguration());
            var conn1 = new Connector
            {
                Name = "connName1",
                IsActive = true
            };

            db.Connector.Add(conn1);
            db.SaveChanges();

            IAppAdminService appAdminService = new AppAdminService(db);
            var connector = appAdminService.GetConnectionById(conn1.Id);

            Assert.AreEqual(conn1.Id, connector.Id);
            Assert.AreEqual("connName1", connector.Name);
        }


        [TestMethod]
        [ExpectedException(typeof(ConnectorNotFoundException))]
        public void ConnectorNotFoundById()
        {
            var db = new AppDbContext(appDbOptions, new ContextEntityConfiguration());
            db.Connector.Add(new Connector
            {
                Id = "12",
                Name = "connName1",
                IsActive = true
            });
            db.SaveChanges();

            IAppAdminService appAdminService = new AppAdminService(db);
            _ = appAdminService.GetConnectionById("45");
        }

        [TestMethod]
        [ExpectedException(typeof(ConnectorNotFoundException))]
        public void ConnectorNotFoundByIdDisabled()
        {
            var db = new AppDbContext(appDbOptions, new ContextEntityConfiguration());
            db.Connector.Add(new Connector
            {
                Id = "45",
                Name = "connName1",
                IsActive = false
            });
            db.SaveChanges();

            IAppAdminService appAdminService = new AppAdminService(db);
            _ = appAdminService.GetConnectionById("45");
        }
    }
}
