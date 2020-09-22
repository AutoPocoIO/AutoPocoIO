using AutoPocoIO.Context;
using AutoPocoIO.Extensions;
using AutoPocoIO.test.TestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoPocoIO.test.Extensions
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class JoinAndGroupJoinParameterNullTests : DbAccessUnitTestBase
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JoinCheckOuterNotNull()
        {
            var db = new AppDbContext(AppDbOptions);
            IQueryable<string> obj = null;
            obj.Join(db.Connector, "a", "a", "a");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JoinCheckInnerNotNull()
        {
            var db = new AppDbContext(AppDbOptions);
            IQueryable<string> obj = null;
            db.Connector.Join(obj, "a", "a", "a");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JoinCheckOuterSelectorNotNull()
        {
            var db = new AppDbContext(AppDbOptions);
            db.Connector.Join(db.UserJoin, null, "a", "a");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JoinCheckInnerSelectorNotNull()
        {
            var db = new AppDbContext(AppDbOptions);
            db.Connector.Join(db.UserJoin, "a", null, "a");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JoinCheckResultSelectorNotNull()
        {
            var db = new AppDbContext(AppDbOptions);
            db.Connector.Join(db.UserJoin, "a", "a", null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void JoinCheckOuterSelectorNotEmpty()
        {
            var db = new AppDbContext(AppDbOptions);
            db.Connector.Join(db.UserJoin, "", "a", "a");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void JoinCheckInnerSelectorNotEmpty()
        {
            var db = new AppDbContext(AppDbOptions);
            db.Connector.Join(db.UserJoin, "a", "", "a");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void JoinCheckResultSelectorNotEmpty()
        {
            var db = new AppDbContext(AppDbOptions);
            db.Connector.Join(db.UserJoin, "a", "a", "");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GroupJoinCheckOuterNotNull()
        {
            var db = new AppDbContext(AppDbOptions);
            IQueryable<string> obj = null;
            obj.GroupJoin(db.Connector, "a", "a", "a");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GroupJoinCheckInnerNotNull()
        {
            var db = new AppDbContext(AppDbOptions);
            IQueryable<string> obj = null;
            db.Connector.GroupJoin(obj, "a", "a", "a");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GroupJoinCheckOuterSelectorNotNull()
        {
            var db = new AppDbContext(AppDbOptions);
            db.Connector.GroupJoin(db.UserJoin, null, "a", "a");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GroupJoinCheckInnerSelectorNotNull()
        {
            var db = new AppDbContext(AppDbOptions);
            db.Connector.GroupJoin(db.UserJoin, "a", null, "a");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GroupJoinCheckResultSelectorNotNull()
        {
            var db = new AppDbContext(AppDbOptions);
            db.Connector.GroupJoin(db.UserJoin, "a", "a", null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GroupJoinCheckOuterSelectorNotEmpty()
        {
            var db = new AppDbContext(AppDbOptions);
            db.Connector.GroupJoin(db.UserJoin, "", "a", "a");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GroupJoinCheckInnerSelectorNotEmpty()
        {
            var db = new AppDbContext(AppDbOptions);
            db.Connector.GroupJoin(db.UserJoin, "a", "", "a");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GroupJoinCheckResultSelectorNotEmpty()
        {
            var db = new AppDbContext(AppDbOptions);
            db.Connector.GroupJoin(db.UserJoin, "a", "a", "");
        }
    }
}
