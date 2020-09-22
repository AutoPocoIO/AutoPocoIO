using AutoPocoIO.Context;
using AutoPocoIO.EntityConfiguration;
using AutoPocoIO.Extensions;
using AutoPocoIO.Models;
using AutoPocoIO.test.TestHelpers;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace AutoPocoIO.test.Extensions
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class ODataApplyQueryTests : DbAccessUnitTestBase
    {
        private IDictionary<string, string> queryString;
        private void SetQueryString(params (string key, string value)[] query)
        {
            queryString = new Dictionary<string, string>();
            foreach (var (key, value) in query)
            {
                queryString.Add(key, value);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ODataException))]
        public void ThrowExceptionIfRequestingOverLimit()
        {
            SetQueryString(("$top", "6"));
            var db = new AppDbContext(AppDbOptions, new VersionedContextEntityConfiguration());
            db.Connector.ApplyQuery(2, queryString);
            Assert.Fail("Apply Query should have failed");
        }

        [TestMethod]
        public void TakeTopIfUnderLimit()
        {
            SetQueryString(("$top", "1"));
            var db = new AppDbContext(AppDbOptions, new VersionedContextEntityConfiguration());
            db.Connector.AddRange(new Connector(), new Connector());
            db.SaveChanges();

            var result = db.Connector.ApplyQuery(2, queryString);
            Assert.AreEqual(1, result.Count());
        }

        [TestMethod]
        public void SetTopToLimitIfNotInQueryString()
        {
            SetQueryString();
            var db = new AppDbContext(AppDbOptions, new VersionedContextEntityConfiguration());
            db.Connector.AddRange(new Connector(), new Connector(), new Connector());
            db.SaveChanges();

            var result = db.Connector.ApplyQuery(2, queryString);
            Assert.AreEqual(2, result.Count());
        }

        [TestMethod]
        public void AppenedTopAndKeepOtherParams()
        {
            SetQueryString(("abc", "123"));
            var db = new AppDbContext(AppDbOptions, new VersionedContextEntityConfiguration());
            db.Connector.AddRange(new Connector(), new Connector(), new Connector());
            db.SaveChanges();

            var result = db.Connector.ApplyQuery(2, queryString);
            Assert.AreEqual(2, result.Count());
            // Assert.AreEqual("http://test.com/site?abc=123&$top=2", url);
        }

        [TestMethod]
        public void OrderBy()
        {
            SetQueryString(("$orderby", "name"));
            var db = new AppDbContext(AppDbOptions, new VersionedContextEntityConfiguration());
            db.Connector.AddRange(
                new Connector { Name = "c" },
                new Connector { Name = "a" },
                new Connector { Name = "b" });
            db.SaveChanges();

            IQueryable<dynamic> result = db.Connector.ApplyQuery(1, queryString);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual<string>("a", result.First().Name);
        }

        [TestMethod]
        public void OrderByWithTop2()
        {
            SetQueryString(("$orderby", "name"), ("$top", "2"));
            var db = new AppDbContext(AppDbOptions, new VersionedContextEntityConfiguration());
            db.Connector.AddRange(
                new Connector { Name = "c" },
                new Connector { Name = "a" },
                new Connector { Name = "b" });
            db.SaveChanges();

            IQueryable<dynamic> result = db.Connector.ApplyQuery(3, queryString);
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual<string>("a", result.First().Name);
        }

        [TestMethod]
        public void GetCount()
        {
            SetQueryString(("$count", "true"));
            var db = new AppDbContext(AppDbOptions, new VersionedContextEntityConfiguration());
            db.Connector.AddRange(
                new Connector { Name = "c" },
                new Connector { Name = "a" },
                new Connector { Name = "b" });
            db.SaveChanges();

            IQueryable<dynamic> result = db.Connector.ApplyQuery(30, queryString);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual<int>(3, result.First().Count);
        }

        [TestMethod]
        public void GetCountIgnoreLimit()
        {
            SetQueryString(("$count", "true"));
            var db = new AppDbContext(AppDbOptions, new VersionedContextEntityConfiguration());
            db.Connector.AddRange(
                new Connector { Name = "c" },
                new Connector { Name = "a" },
                new Connector { Name = "b" });
            db.SaveChanges();

            IQueryable<dynamic> result = db.Connector.ApplyQuery(1, queryString);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual<int>(3, result.First().Count);
        }

        [TestMethod]
        public void GetCountWithTop()
        {
            SetQueryString(("$count", "true"), ("$top", "2"));
            var db = new AppDbContext(AppDbOptions, new VersionedContextEntityConfiguration());
            db.Connector.AddRange(
                new Connector { Name = "c" },
                new Connector { Name = "a" },
                new Connector { Name = "b" });
            db.SaveChanges();

            IQueryable<dynamic> result = db.Connector.ApplyQuery(30, queryString);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual<int>(2, result.First().Count);
        }

        [TestMethod]
        public void GetCountWithFilter()
        {
            SetQueryString(("$count", "true"), ("$filter", "name eq 'a'"));
            var db = new AppDbContext(AppDbOptions, new VersionedContextEntityConfiguration());
            db.Connector.AddRange(
                new Connector { Name = "c" },
                new Connector { Name = "a" },
                new Connector { Name = "b" });
            db.SaveChanges();

            IQueryable<dynamic> result = db.Connector.ApplyQuery(30, queryString);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual<int>(1, result.First().Count);
        }
    }
}
