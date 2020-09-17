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
        private class ViewModel1
        {
            public string Id { get; set; }
            public string Id2 { get; set; }
            public string Name { get; set; }
        }

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
            var list = new List<ViewModel1>
            {
                new ViewModel1
                {
                     Id = "1",
                     Id2 = "2",
                     Name = "Name1"
                },
                new ViewModel1
                {
                     Id = "2",
                     Id2 = "2",
                     Name = "Name2"
                }
            }.AsQueryable();


            var results = list.GroupJoin<dynamic>(list, "outer.Id", "inner.Id2", "new(outer.Name, group as ResouceTypes)")
                .ToList();

            Assert.AreEqual(2, results.Count());
            Assert.AreEqual("Name1", results.First().Name);
            Assert.AreEqual("Name2", results.Last().Name);
            Assert.AreEqual(0, ((IEnumerable<dynamic>)results.First().ResouceTypes).Count());
            Assert.AreEqual(2, ((IEnumerable<dynamic>)results.Last().ResouceTypes).Count());
        }
    }
}
