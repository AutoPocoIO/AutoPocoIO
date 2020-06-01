using AutoPocoIO.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;

namespace AutoPocoIO.test.Exceptions
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class StoreProcedureNotFoundExceptionTests
    {
        [TestMethod]
        public void IsBaseException()
        {
            var ex = new StoreProcedureNotFoundException("a", "b");
            Assert.IsInstanceOfType(ex, typeof(BaseCaughtException));
        }

        [TestMethod]
        public void ErrorMessageIsPopulated()
        {
            var ex = new StoreProcedureNotFoundException("sch", "sp");
            Assert.AreEqual("Stored Procedure 'sch.sp' not found.", ex.Message);
        }

        [TestMethod]
        public void HttpErrorMessageIsPopulated()
        {
            var ex = new StoreProcedureNotFoundException("sch", "sp");
            Assert.AreEqual("StoreProcedureNotFound", ex.HttpErrorMessage);
        }

        [TestMethod]
        public void HttpStatusCodeIsPopulated()
        {
            var ex = new StoreProcedureNotFoundException("sch", "sp");
            Assert.AreEqual(HttpStatusCode.InternalServerError, ex.ResponseCode);
        }
    }
}