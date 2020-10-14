using AutoPocoIO.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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
            Guid id = Guid.NewGuid();
            var ex = new ConnectorNotFoundException(id);
            Assert.AreEqual($"Connector with Id '{id}' not found.", ex.Message);
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
            Guid id = Guid.NewGuid();
            var ex = new ConnectorNotFoundException(id);
            Assert.AreEqual(HttpStatusCode.BadRequest, ex.ResponseCode);
        }
    }
}