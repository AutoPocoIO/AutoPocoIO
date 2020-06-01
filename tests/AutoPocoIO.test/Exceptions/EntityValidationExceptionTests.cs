using AutoPocoIO.Exceptions;
using System.Net;
using Xunit;

namespace AutoPocoIO.test.Exceptions
{
    [Trait("Category", TestCategories.Unit)]
    public class EntityValidationExceptionTests
    {
        [FactWithName]
        public void IsBaseException()
        {
            var ex = new EntityValidationException("msg");
            Assert.IsAssignableFrom<BaseCaughtException>(ex);
        }

        [FactWithName]
        public void ErrorMessageIsPopulated()
        {
            var ex = new EntityValidationException("msg");
            Assert.Equal("msg", ex.Message);
        }

        [FactWithName]
        public void HttpErrorMessageIsPopulated()
        {
            var ex = new EntityValidationException("msg");
            Assert.Equal("InternalServerError", ex.HttpErrorMessage);
        }

        [FactWithName]
        public void HttpStatusCodeIsPopulated()
        {
            var ex = new EntityValidationException("msg");
            Assert.Equal(HttpStatusCode.InternalServerError, ex.ResponseCode);
        }
    }
}