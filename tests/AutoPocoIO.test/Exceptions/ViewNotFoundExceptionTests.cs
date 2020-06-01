using AutoPocoIO.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;

namespace AutoPocoIO.test.Exceptions
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class ViewNotFoundExceptionTests
    {
        [TestMethod]
        public void IsBaseException()
        {
            var ex = new ViewNotFoundException("a", "b");
            Assert.IsInstanceOfType(ex, typeof(BaseCaughtException));
        }

        [TestMethod]
        public void ErrorMessageIsPopulated()
        {
            var ex = new ViewNotFoundException("sch", "vw");
            Assert.AreEqual("View 'sch.vw' not found.", ex.Message);
        }

        [TestMethod]
        public void HttpErrorMessageIsPopulated()
        {
            var ex = new ViewNotFoundException("sch", "vw");
            Assert.AreEqual("ViewNotFound", ex.HttpErrorMessage);
        }

        [TestMethod]
        public void HttpStatusCodeIsPopulated()
        {
            var ex = new ViewNotFoundException("sch", "vw");
            Assert.AreEqual(HttpStatusCode.InternalServerError, ex.ResponseCode);
        }
    }
}