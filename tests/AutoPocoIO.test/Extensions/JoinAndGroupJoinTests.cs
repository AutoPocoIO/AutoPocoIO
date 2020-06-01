using AutoPocoIO.Context;
using AutoPocoIO.Extensions;
using AutoPocoIO.Models;
using AutoPocoIO.test.TestHelpers;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace AutoPocoIO.test.Extensions
{
    
    [Trait("Category", TestCategories.Unit)]
    public class JoinAndGroupJoinTests : DbAccessUnitTestBase
    {
        [FactWithName]
        public void JoinNoResults()
        {
            var db = new AppDbContext(AppDbOptions);
            var results = db.Connector.Join<dynamic>(db.UserJoin, "Id", "Id", "new(outer.Name, inner.Alias)");
            Assert.Equal(0, results.Count());
        }

        [FactWithName]
        public void JoinWithStringValues()
        {
            var db = new AppDbContext(AppDbOptions);
            db.Connector.Add(new Connector
            {
                Id = 1,
                Name = "Name2"
            });
            db.UserJoin.Add(new UserJoin
            {
                Id = 1,
                Alias = "Alias"
            });
            db.SaveChanges();

            var results = db.Connector.Join<dynamic>(db.UserJoin, "Id", "Id", "new(outer.Name, inner.Alias)");

            Assert.Equal(1, results.Count());
            Assert.Equal("Name2", results.First().Name);
            Assert.Equal("Alias", results.First().Alias);

        }

        [FactWithName]
        public void GroupJoinWithStringValues()
        {
            var db = new AppDbContext(AppDbOptions);
            db.Connector.Add(new Connector
            {
                Id = 1,
                ResourceType = 2,
                Name = "Name1"
            });
            db.Connector.Add(new Connector
            {
                Id = 2,
                ResourceType = 2,
                Name = "Name2"
            });
            db.SaveChanges();

            var results = db.Connector.GroupJoin<dynamic>(db.Connector, "outer.Id", "inner.ResourceType", "new(outer.Name, group as ResouceTypes)")
                .ToList();

            Assert.Equal(2, results.Count());
            Assert.Equal("Name1", results.First().Name);
            Assert.Equal("Name2", results.Last().Name);
            Assert.Empty((IEnumerable<dynamic>)results.First().ResouceTypes);
            Assert.Equal(2, ((IEnumerable<dynamic>)results.Last().ResouceTypes).Count());
        }
    }
}
