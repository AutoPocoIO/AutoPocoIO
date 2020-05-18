using AutoPocoIO.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;

namespace AutoPocoIO.test.Exceptions
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class NoPrimaryKeyFoundExceptionTests
    {
        private class TestEx : NoPrimaryKeyFoundException
        {
            public TestEx() : base(null, new System.Runtime.Serialization.StreamingContext())
            {

            }
        }

        [TestMethod]
        public void IsBaseException()
        {
            var ex = new NoPrimaryKeyFoundException("a", "b");
            Assert.IsInstanceOfType(ex, typeof(BaseCaughtException));
        }

        [TestMethod]
        public void ErrorMessageIsPopulated()
        {
            var ex = new NoPrimaryKeyFoundException("entity", "tbl");
            Assert.AreEqual("Virtual Entity 'entity' references tbl and does not contain a primary key.", ex.Message);
        }

        [TestMethod]
        public void HttpErrorMessageIsPopulated()
        {
            var ex = new NoPrimaryKeyFoundException("entity", "tbl");
            Assert.AreEqual("EntityKeyNotFound", ex.HttpErrorMessage);
        }

        [TestMethod]
        public void HttpStatusCodeIsPopulated()
        {
            var ex = new NoPrimaryKeyFoundException("entity", "tbl");
            Assert.AreEqual(HttpStatusCode.InternalServerError, ex.ResponseCode);
        }

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void SerilizationConstructorNotImplmented()
        {
            _ = new TestEx();
        }
    }
}
