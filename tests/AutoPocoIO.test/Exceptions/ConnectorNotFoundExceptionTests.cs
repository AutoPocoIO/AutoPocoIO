using AutoPocoIO.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;

namespace AutoPocoIO.test.Exceptions
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class ConnectorNotFoundExceptionTests
    {
        [TestMethod]
        public void IsBaseException()
        {
            var ex = new ConnectorNotFoundException("conn");
            Assert.IsInstanceOfType(ex, typeof(BaseCaughtException));
        }

        [TestMethod]
        public void ErrorMessageIsPopulated()
        {
            var ex = new ConnectorNotFoundException("conn");
            Assert.AreEqual("Connector 'conn' not found.", ex.Message);
        }

        [TestMethod]
        public void ErrorMessageWithIdIsPopulated()
        {
            var ex = new ConnectorNotFoundException(12);
            Assert.AreEqual("Connector with Id '12' not found.", ex.Message);
        }

        [TestMethod]
        public void HttpErrorMessageIsPopulated()
        {
            var ex = new ConnectorNotFoundException("conn");
            Assert.AreEqual("ConnectorNotFound", ex.HttpErrorMessage);
        }

        [TestMethod]
        public void HttpStatusCodeIsPopulated()
        {
            var ex = new ConnectorNotFoundException("conn");
            Assert.AreEqual(HttpStatusCode.BadRequest, ex.ResponseCode);
        }

        [TestMethod]
        public void HttpStatusCodeWithIdIsPopulated()
        {
            var ex = new ConnectorNotFoundException(12);
            Assert.AreEqual(HttpStatusCode.BadRequest, ex.ResponseCode);
        }
    }
}