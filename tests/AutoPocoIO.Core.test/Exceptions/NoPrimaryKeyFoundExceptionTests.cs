using AutoPocoIO.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;

namespace AutoPocoIO.test.Exceptions
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class NoPrimaryKeyFoundExceptionTests
    {
        [TestMethod]
        public void IsBaseException()
        {
            var ex = new NoPrimaryKeyFoundException("a");
            Assert.IsInstanceOfType(ex, typeof(BaseCaughtException));
        }

        [TestMethod]
        public void ErrorMessageIsPopulated()
        {
            var ex = new NoPrimaryKeyFoundException("entity");
            Assert.AreEqual("Entity 'entity' does not contain a primary key.", ex.Message);
        }

        [TestMethod]
        public void HttpErrorMessageIsPopulated()
        {
            var ex = new NoPrimaryKeyFoundException("entity");
            Assert.AreEqual("EntityKeyNotFound", ex.HttpErrorMessage);
        }

        [TestMethod]
        public void HttpStatusCodeIsPopulated()
        {
            var ex = new NoPrimaryKeyFoundException("entity");
            Assert.AreEqual(HttpStatusCode.InternalServerError, ex.ResponseCode);
        }
    }
}
