using AutoPocoIO.Context;
using AutoPocoIO.Extensions;
using AutoPocoIO.test.TestHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace AutoPocoIO.test.Extensions
{
    [Trait("Category", TestCategories.Unit)]
    public class JoinAndGroupJoinParameterNullTests : DbAccessUnitTestBase
    {
        [FactWithName]
        public void JoinCheckOuterNotNull()
        {
            var db = new AppDbContext(AppDbOptions);
            IQueryable<string> obj = null;
             void act() => obj.Join(db.Connector, "a", "a", "a");
            Assert.Throws<ArgumentNullException>(act);
        }

        [FactWithName]
        public void JoinCheckInnerNotNull()
        {
            var db = new AppDbContext(AppDbOptions);
            IQueryable<string> obj = null;
             void act() => db.Connector.Join(obj, "a", "a", "a");
            Assert.Throws<ArgumentNullException>(act);
        }

        [FactWithName]
        public void JoinCheckOuterSelectorNotNull()
        {
            var db = new AppDbContext(AppDbOptions);
             void act() => db.Connector.Join(db.UserJoin, null, "a", "a");
            Assert.Throws<ArgumentNullException>(act);
        }

        [FactWithName]
        public void JoinCheckInnerSelectorNotNull()
        {
            var db = new AppDbContext(AppDbOptions);
             void act() => db.Connector.Join(db.UserJoin, "a", null, "a");
            Assert.Throws<ArgumentNullException>(act);
        }

        [FactWithName]
        public void JoinCheckResultSelectorNotNull()
        {
            var db = new AppDbContext(AppDbOptions);
             void act() => db.Connector.Join(db.UserJoin, "a", "a", null);
            Assert.Throws<ArgumentNullException>(act);
        }

        [FactWithName]
        public void JoinCheckOuterSelectorNotEmpty()
        {
            var db = new AppDbContext(AppDbOptions);
             void act() => db.Connector.Join(db.UserJoin, "", "a", "a");
            Assert.Throws<ArgumentException>(act);
        }

        [FactWithName]
        public void JoinCheckInnerSelectorNotEmpty()
        {
            var db = new AppDbContext(AppDbOptions);
             void act() => db.Connector.Join(db.UserJoin, "a", "", "a");
            Assert.Throws<ArgumentException>(act);
        }

        [FactWithName]
        public void JoinCheckResultSelectorNotEmpty()
        {
            var db = new AppDbContext(AppDbOptions);
             void act() => db.Connector.Join(db.UserJoin, "a", "a", "");
            Assert.Throws<ArgumentException>(act);
        }

        [FactWithName]
        public void GroupJoinCheckOuterNotNull()
        {
            var db = new AppDbContext(AppDbOptions);
            IQueryable<string> obj = null;
             void act() => obj.GroupJoin(db.Connector, "a", "a", "a");
            Assert.Throws<ArgumentNullException>(act);
        }

        [FactWithName]
        public void GroupJoinCheckInnerNotNull()
        {
            var db = new AppDbContext(AppDbOptions);
            IQueryable<string> obj = null;
             void act() => db.Connector.GroupJoin(obj, "a", "a", "a");
            Assert.Throws<ArgumentNullException>(act);
        }

        [FactWithName]
        public void GroupJoinCheckOuterSelectorNotNull()
        {
            var db = new AppDbContext(AppDbOptions);
             void act() => db.Connector.GroupJoin(db.UserJoin, null, "a", "a");
            Assert.Throws<ArgumentNullException>(act);
        }

        [FactWithName]
        public void GroupJoinCheckInnerSelectorNotNull()
        {
            var db = new AppDbContext(AppDbOptions);
             void act() => db.Connector.GroupJoin(db.UserJoin, "a", null, "a");
            Assert.Throws<ArgumentNullException>(act);
        }

        [FactWithName]
        public void GroupJoinCheckResultSelectorNotNull()
        {
            var db = new AppDbContext(AppDbOptions);
             void act() => db.Connector.GroupJoin(db.UserJoin, "a", "a", null);
            Assert.Throws<ArgumentNullException>(act);
        }

        [FactWithName]
        public void GroupJoinCheckOuterSelectorNotEmpty()
        {
            var db = new AppDbContext(AppDbOptions);
             void act() => db.Connector.GroupJoin(db.UserJoin, "", "a", "a");
            Assert.Throws<ArgumentException>(act);
        }

        [FactWithName]
        public void GroupJoinCheckInnerSelectorNotEmpty()
        {
            var db = new AppDbContext(AppDbOptions);
             void act() => db.Connector.GroupJoin(db.UserJoin, "a", "", "a");
            Assert.Throws<ArgumentException>(act);
        }

        [FactWithName]
        public void GroupJoinCheckResultSelectorNotEmpty()
        {
            var db = new AppDbContext(AppDbOptions);
             void act() => db.Connector.GroupJoin(db.UserJoin, "a", "a", "");
            Assert.Throws<ArgumentException>(act);
        }
    }
}
