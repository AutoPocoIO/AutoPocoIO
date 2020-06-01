using AutoPocoIO.Exceptions;
using System.Net;
using Xunit;

namespace AutoPocoIO.test.Exceptions
{
    [Trait("Category", TestCategories.Unit)]
    public class ConnectorNotFoundExceptionTests
    {
        [FactWithName]
        public void IsBaseException()
        {
            var ex = new ConnectorNotFoundException("conn");
            Assert.IsAssignableFrom<BaseCaughtException>(ex);
        }

        [FactWithName]
        public void ErrorMessageIsPopulated()
        {
            var ex = new ConnectorNotFoundException("conn");
            Assert.Equal("Connector 'conn' not found.", ex.Message);
        }

        [FactWithName]
        public void ErrorMessageWithIdIsPopulated()
        {
            var ex = new ConnectorNotFoundException(12);
            Assert.Equal("Connector with Id '12' not found.", ex.Message);
        }

        [FactWithName]
        public void HttpErrorMessageIsPopulated()
        {
            var ex = new ConnectorNotFoundException("conn");
            Assert.Equal("ConnectorNotFound", ex.HttpErrorMessage);
        }

        [FactWithName]
        public void HttpStatusCodeIsPopulated()
        {
            var ex = new ConnectorNotFoundException("conn");
            Assert.Equal(HttpStatusCode.BadRequest, ex.ResponseCode);
        }

        [FactWithName]
        public void HttpStatusCodeWithIdIsPopulated()
        {
            var ex = new ConnectorNotFoundException(12);
            Assert.Equal(HttpStatusCode.BadRequest, ex.ResponseCode);
        }
    }
}