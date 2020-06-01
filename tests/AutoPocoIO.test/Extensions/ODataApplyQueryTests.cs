using AutoPocoIO.Context;
using AutoPocoIO.Extensions;
using AutoPocoIO.Models;
using AutoPocoIO.test.TestHelpers;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace AutoPocoIO.test.Extensions
{
    [Trait("Category", TestCategories.Unit)]
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

        [FactWithName]
        public void ThrowExceptionIfRequestingOverLimit()
        {
            SetQueryString(("$top", "6"));
            var db = new AppDbContext(AppDbOptions);
             void act() => db.Connector.ApplyQuery(2, queryString);
            Assert.Throws<ODataException>(act);
        }

        [FactWithName]
        public void TakeTopIfUnderLimit()
        {
            SetQueryString(("$top", "1"));
            var db = new AppDbContext(AppDbOptions);
            db.Connector.AddRange(new Connector(), new Connector());
            db.SaveChanges();

            var result = db.Connector.ApplyQuery(2, queryString);
            Assert.Equal(1, result.Count());
        }

        [FactWithName]
        public void SetTopToLimitIfNotInQueryString()
        {
            SetQueryString();
            var db = new AppDbContext(AppDbOptions);
            db.Connector.AddRange(new Connector(), new Connector(), new Connector());
            db.SaveChanges();

            var result = db.Connector.ApplyQuery(2, queryString);
            Assert.Equal(2, result.Count());
        }

        [FactWithName]
        public void AppenedTopAndKeepOtherParams()
        {
            SetQueryString(("abc", "123"));
            var db = new AppDbContext(AppDbOptions);
            db.Connector.AddRange(new Connector(), new Connector(), new Connector());
            db.SaveChanges();

            var result = db.Connector.ApplyQuery(2, queryString);
            Assert.Equal(2, result.Count());
            // Assert.Equal("http://test.com/site?abc=123&$top=2", url);
        }

        [FactWithName]
        public void OrderBy()
        {
            SetQueryString(("$orderby", "name"));
            var db = new AppDbContext(AppDbOptions);
            db.Connector.AddRange(
                new Connector { Name = "c" },
                new Connector { Name = "a" },
                new Connector { Name = "b" });
            db.SaveChanges();

            IQueryable<dynamic> result = db.Connector.ApplyQuery(1, queryString);
            Assert.Equal(1, result.Count());
            Assert.Equal("a", result.First().Name);
        }

        [FactWithName]
        public void OrderByWithTop2()
        {
            SetQueryString(("$orderby", "name"), ("$top", "2"));
            var db = new AppDbContext(AppDbOptions);
            db.Connector.AddRange(
                new Connector { Name = "c" },
                new Connector { Name = "a" },
                new Connector { Name = "b" });
            db.SaveChanges();

            IQueryable<dynamic> result = db.Connector.ApplyQuery(3, queryString);
            Assert.Equal(2, result.Count());
            Assert.Equal("a", result.First().Name);
        }

        [FactWithName]
        public void GetCount()
        {
            SetQueryString(("$count", "true"));
            var db = new AppDbContext(AppDbOptions);
            db.Connector.AddRange(
                new Connector { Name = "c" },
                new Connector { Name = "a" },
                new Connector { Name = "b" });
            db.SaveChanges();

            IQueryable<dynamic> result = db.Connector.ApplyQuery(30, queryString);
            Assert.Equal(1, result.Count());
            Assert.Equal<int>(3, result.First().Count);
        }

        [FactWithName]
        public void GetCountIgnoreLimit()
        {
            SetQueryString(("$count", "true"));
            var db = new AppDbContext(AppDbOptions);
            db.Connector.AddRange(
                new Connector { Name = "c" },
                new Connector { Name = "a" },
                new Connector { Name = "b" });
            db.SaveChanges();

            IQueryable<dynamic> result = db.Connector.ApplyQuery(1, queryString);
            Assert.Equal(1, result.Count());
            Assert.Equal<int>(3, result.First().Count);
        }

        [FactWithName]
        public void GetCountWithTop()
        {
            SetQueryString(("$count", "true"), ("$top", "2"));
            var db = new AppDbContext(AppDbOptions);
            db.Connector.AddRange(
                new Connector { Name = "c" },
                new Connector { Name = "a" },
                new Connector { Name = "b" });
            db.SaveChanges();

            IQueryable<dynamic> result = db.Connector.ApplyQuery(30, queryString);
            Assert.Equal(1, result.Count());
            Assert.Equal<int>(2, result.First().Count);
        }

        [FactWithName]
        public void GetCountWithFilter()
        {
            SetQueryString(("$count", "true"), ("$filter", "name eq 'a'"));
            var db = new AppDbContext(AppDbOptions);
            db.Connector.AddRange(
                new Connector { Name = "c" },
                new Connector { Name = "a" },
                new Connector { Name = "b" });
            db.SaveChanges();

            IQueryable<dynamic> result = db.Connector.ApplyQuery(30, queryString);
            Assert.Equal(1, result.Count());
            Assert.Equal<int>(1, result.First().Count);
        }
    }
}
