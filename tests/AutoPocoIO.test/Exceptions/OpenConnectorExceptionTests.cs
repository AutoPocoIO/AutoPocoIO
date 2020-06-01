using AutoPocoIO.Exceptions;
using System.Net;
using Xunit;

namespace AutoPocoIO.test.Exceptions
{
    [Trait("Category", TestCategories.Unit)]
    public class OpenConnectorExceptionTests
    {
        [FactWithName]
        public void IsBaseException()
        {
            var ex = new OpenConnectorException("conn");
            Assert.IsAssignableFrom<BaseCaughtException>(ex);
        }

        [FactWithName]
        public void ErrorMessageIsPopulated()
        {
            var ex = new OpenConnectorException("conn");
            Assert.Equal("An error occurred opening the connector 'conn'.", ex.Message);
        }

        [FactWithName]
        public void HttpErrorMessageIsPopulated()
        {
            var ex = new OpenConnectorException("conn");
            Assert.Equal("ConnectionError", ex.HttpErrorMessage);
        }


        [FactWithName]
        public void HttpStatusCodeIsPopulated()
        {
            var ex = new OpenConnectorException("conn");
            Assert.Equal(HttpStatusCode.InternalServerError, ex.ResponseCode);
        }
    }
}