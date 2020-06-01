using AutoPocoIO.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;

namespace AutoPocoIO.test.Exceptions
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class TableNotFoundExceptionTests
    {
        [TestMethod]
        public void IsBaseException()
        {
            var ex = new TableNotFoundException("db", "a", "b");
            Assert.IsInstanceOfType(ex, typeof(BaseCaughtException));
        }

        [TestMethod]
        public void ErrorMessageIsPopulated()
        {
            var ex = new TableNotFoundException("db", "sch", "tbl");
            Assert.AreEqual("Table 'db.sch.tbl' not found.", ex.Message);
        }

        [TestMethod]
        public void HttpErrorMessageIsPopulated()
        {
            var ex = new TableNotFoundException("db", "sch", "tbl");
            Assert.AreEqual("TableNotFound", ex.HttpErrorMessage);
        }

        [TestMethod]
        public void HttpStatusCodeIsPopulated()
        {
            var ex = new TableNotFoundException("db", "sch", "tbl");
            Assert.AreEqual(HttpStatusCode.InternalServerError, ex.ResponseCode);
        }
    }
}