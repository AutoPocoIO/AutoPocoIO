using AutoPocoIO.Context;
using AutoPocoIO.Dashboard.Repos;
using AutoPocoIO.Dashboard.ViewModels;
using AutoPocoIO.EntityConfiguration;
using AutoPocoIO.Factories;
using AutoPocoIO.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace AutoPocoIO.test.Dashboard.Repos
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class ConnectorRepoValidateTests
    {
        private AppDbContext db;
        private ConnectorRepo repo;
        private Dictionary<string, string> errors;
        [TestInitialize]
        public void Init()
        {
            var appDbOptions = new DbContextOptionsBuilder<AppDbContext>()
              .UseInMemoryDatabase(databaseName: "appDb" + Guid.NewGuid().ToString())
              .Options;

            db = new AppDbContext(appDbOptions, new ContextEntityConfiguration());
            repo = new ConnectorRepo(db, Mock.Of<IConnectionStringFactory>(), new IOperationResource[] { });
            errors = new Dictionary<string, string>();
        }

        [TestMethod]
        public void ValidateClearsErrorsFirst()
        {
            errors["otherError"] = "val1";

            repo.Validate(new ConnectorViewModel(), errors);
            Assert.IsFalse(errors.ContainsKey("otherError"));
        }

        [TestMethod]
        public void ConnectorNameRequired()
        {
            repo.Validate(new ConnectorViewModel(), errors);
            Assert.AreEqual("Connector Name is required.", errors["Name"]);
        }

        [TestMethod]
        public void ConnectorNameMaxLengthExceeded()
        {
            repo.Validate(new ConnectorViewModel { Name = new string('a', 51) }, errors);
            Assert.AreEqual("Connector Name has a max length of 50.  51 was submitted.", errors["Name"]);
        }

        [TestMethod]
        public void ConnectorNameUniqueError()
        {
            db.Connector.Add(new Models.Connector { Name = "abc" });
            db.SaveChanges();

            repo.Validate(new ConnectorViewModel { Name = "ABC" }, errors);
            Assert.AreEqual("The Connector Name ABC already exists.", errors["Name"]);
        }

        [TestMethod]
        public void ResourceTypeRequired()
        {
            repo.Validate(new ConnectorViewModel(), errors);
            Assert.AreEqual("Resource Type is required.", errors["ResourceType"]);
        }

        [TestMethod]
        public void SchemaRequired()
        {
            repo.Validate(new ConnectorViewModel(), errors);
            Assert.AreEqual("Schema Name is required.", errors["Schema"]);
        }

        [TestMethod]
        public void SchemaMaxLengthExceeded()
        {
            repo.Validate(new ConnectorViewModel { Schema = new string('a', 51) }, errors);
            Assert.AreEqual("Schema Name has a max length of 50.  51 was submitted.", errors["Schema"]);
        }

        [TestMethod]
        public void CatalogeRequired()
        {
            repo.Validate(new ConnectorViewModel(), errors);
            Assert.AreEqual("Database Name is required.", errors["InitialCatalog"]);
        }

        [TestMethod]
        public void CatalogeMaxLengthExceeded()
        {
            repo.Validate(new ConnectorViewModel { InitialCatalog = new string('a', 51) }, errors);
            Assert.AreEqual("Database Name has a max length of 50.  51 was submitted.", errors["InitialCatalog"]);
        }

        [TestMethod]
        public void UserIdMaxLengthExceeded()
        {
            repo.Validate(new ConnectorViewModel { UserId = new string('a', 51) }, errors);
            Assert.AreEqual("User Id has a max length of 50.  51 was submitted.", errors["UserId"]);
        }

        [TestMethod]
        public void DataSourceRequired()
        {
            repo.Validate(new ConnectorViewModel(), errors);
            Assert.AreEqual("Server Name is required.", errors["DataSource"]);
        }

        [TestMethod]
        public void DataSourceMaxLengthExceeded()
        {
            repo.Validate(new ConnectorViewModel { DataSource = new string('a', 51) }, errors);
            Assert.AreEqual("Server Name has a max length of 50.  51 was submitted.", errors["DataSource"]);
        }
        [TestMethod]
        public void RecordLimitRequired()
        {
            repo.Validate(new ConnectorViewModel(), errors);
            Assert.AreEqual("Record Limit is required.", errors["RecordLimit"]);
        }

    }
}
