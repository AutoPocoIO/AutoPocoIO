using AutoPocoIO.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;

namespace AutoPocoIO.test.Exceptions
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class EntityValidationExceptionTests
    {
        [TestMethod]
        public void IsBaseException()
        {
            var ex = new EntityValidationException("msg");
            Assert.IsInstanceOfType(ex, typeof(BaseCaughtException));
        }

        [TestMethod]
        public void ErrorMessageIsPopulated()
        {
            var ex = new EntityValidationException("msg");
            Assert.AreEqual("msg", ex.Message);
        }

        [TestMethod]
        public void HttpErrorMessageIsPopulated()
        {
            var ex = new EntityValidationException("msg");
            Assert.AreEqual("InternalServerError", ex.HttpErrorMessage);
        }

        [TestMethod]
        public void HttpStatusCodeIsPopulated()
        {
            var ex = new EntityValidationException("msg");
            Assert.AreEqual(HttpStatusCode.InternalServerError, ex.ResponseCode);
        }
    }
}