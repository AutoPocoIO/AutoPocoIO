using AutoPocoIO.Context;
using AutoPocoIO.Extensions;
using AutoPocoIO.Models;
using AutoPocoIO.test.TestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
namespace AutoPocoIO.test.Extensions
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class JoinAndGroupJoinTests : DbAccessUnitTestBase
    {
        [TestMethod]
        public void JoinNoResults()
        {
            var db = new AppDbContext(AppDbOptions);
            var results = db.Connector.Join<dynamic>(db.UserJoin, "Id", "Id", "new(outer.Name, inner.Alias)");
            Assert.AreEqual(0, results.Count());
        }

        [TestMethod]
        public void JoinWithStringValues()
        {
            var db = new AppDbContext(AppDbOptions);
            db.Connector.Add(new Connector
            {
                Id = "1",
                Name = "Name2"
            });
            db.UserJoin.Add(new UserJoin
            {
                Id = "1",
                Alias = "Alias"
            });
            db.SaveChanges();

            var results = db.Connector.Join<dynamic>(db.UserJoin, "Id", "Id", "new(outer.Name, inner.Alias)");

            Assert.AreEqual(1, results.Count());
            Assert.AreEqual("Name2", results.First().Name);
            Assert.AreEqual("Alias", results.First().Alias);

        }

        [TestMethod]
        public void GroupJoinWithStringValues()
        {
            var db = new AppDbContext(AppDbOptions);
            db.Connector.Add(new Connector
            {
                Id = "1",
                ResourceType = 2,
                Name = "Name1"
            });
            db.Connector.Add(new Connector
            {
                Id = "2",
                ResourceType = 2,
                Name = "Name2"
            });
            db.SaveChanges();

            var results = db.Connector.GroupJoin<dynamic>(db.Connector, "outer.Id", "inner.ResourceType", "new(outer.Name, group as ResouceTypes)")
                .ToList();

            Assert.AreEqual(2, results.Count());
            Assert.AreEqual("Name1", results.First().Name);
            Assert.AreEqual("Name2", results.Last().Name);
            Assert.AreEqual(0, ((IEnumerable<dynamic>)results.First().ResouceTypes).Count());
            Assert.AreEqual(2, ((IEnumerable<dynamic>)results.Last().ResouceTypes).Count());
        }
    }
}
