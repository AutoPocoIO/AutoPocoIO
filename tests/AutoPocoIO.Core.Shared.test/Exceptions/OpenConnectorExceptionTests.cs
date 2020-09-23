using AutoPocoIO.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;

namespace AutoPocoIO.test.Exceptions
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class OpenConnectorExceptionTests
    {
        [TestMethod]
        public void IsBaseException()
        {
            var ex = new OpenConnectorException("conn");
            Assert.IsInstanceOfType(ex, typeof(BaseCaughtException));
        }

        [TestMethod]
        public void ErrorMessageIsPopulated()
        {
            var ex = new OpenConnectorException("conn");
            Assert.AreEqual("An error occurred opening the connector 'conn'.", ex.Message);
        }

        [TestMethod]
        public void HttpErrorMessageIsPopulated()
        {
            var ex = new OpenConnectorException("conn");
            Assert.AreEqual("ConnectionError", ex.HttpErrorMessage);
        }


        [TestMethod]
        public void HttpStatusCodeIsPopulated()
        {
            var ex = new OpenConnectorException("conn");
            Assert.AreEqual(HttpStatusCode.InternalServerError, ex.ResponseCode);
        }
    }
}