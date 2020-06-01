using AutoPocoIO.Context;
using AutoPocoIO.Exceptions;
using AutoPocoIO.Models;
using AutoPocoIO.Services;
using Microsoft.EntityFrameworkCore;
using System;
using Xunit;

namespace AutoPocoIO.test.Services
{
    [Trait("Category", TestCategories.Unit)]
    public class AppAdminServiceTests
    {
        readonly DbContextOptions<AppDbContext> appDbOptions;
        public AppAdminServiceTests()
        {
            appDbOptions = new DbContextOptionsBuilder<AppDbContext>()
              .UseInMemoryDatabase(databaseName: "appDb" + Guid.NewGuid().ToString())
              .Options;
        }

        [FactWithName]
        public void FindConnector()
        {
            var db = new AppDbContext(appDbOptions);
            db.Connector.Add(new Connector
            {
                Id = 1,
                Name = "connName1",
                IsActive = true
            });
            db.SaveChanges();

            IAppAdminService appAdminService = new AppAdminService(db);

            var connector = appAdminService.GetConnection("connName1");

            Assert.Equal(1, connector.Id);
            Assert.Equal("connName1", connector.Name);
        }


        [FactWithName]
        public void ConnectorNotFound()
        {

            var db = new AppDbContext(appDbOptions);
            db.Connector.Add(new Connector
            {
                Name = "connName1"
            });
            db.SaveChanges();

            IAppAdminService appAdminService = new AppAdminService(db);
             void act() => appAdminService.GetConnection("connName123");
            Assert.Throws<ConnectorNotFoundException>(act);
        }

        [FactWithName]
        public void ConnectorNotActive()
        {

            var db = new AppDbContext(appDbOptions);
            db.Connector.Add(new Connector
            {
                Name = "connName1",
                IsActive = false
            });
            db.SaveChanges();

            IAppAdminService appAdminService = new AppAdminService(db);
             void act() => appAdminService.GetConnection("connName1");
            Assert.Throws<ConnectorNotFoundException>(act);
        }

        [FactWithName]
        public void FindConnectorById()
        {
            var db = new AppDbContext(appDbOptions);
            var conn1 = new Connector
            {
                Name = "connName1",
                IsActive = true
            };

            db.Connector.Add(conn1);
            db.SaveChanges();

            IAppAdminService appAdminService = new AppAdminService(db);
            var connector = appAdminService.GetConnection(conn1.Id);

            Assert.Equal(conn1.Id, connector.Id);
            Assert.Equal("connName1", connector.Name);
        }


        [FactWithName]
        public void ConnectorNotFoundById()
        {
            var db = new AppDbContext(appDbOptions);
            db.Connector.Add(new Connector
            {
                Id = 12,
                Name = "connName1",
                IsActive = true
            });
            db.SaveChanges();

            IAppAdminService appAdminService = new AppAdminService(db);
             void act() => appAdminService.GetConnection(45);
            Assert.Throws<ConnectorNotFoundException>(act);

        }

        [FactWithName]
        public void ConnectorNotFoundByIdDisabled()
        {
            var db = new AppDbContext(appDbOptions);
            db.Connector.Add(new Connector
            {
                Id = 45,
                Name = "connName1",
                IsActive = false
            });
            db.SaveChanges();

            IAppAdminService appAdminService = new AppAdminService(db);
             void act() => appAdminService.GetConnection(45);
            Assert.Throws<ConnectorNotFoundException>(act);
        }
    }
}
