using AutoPocoIO.Exceptions;
using System.Net;
using Xunit;

namespace AutoPocoIO.test.Exceptions
{
    [Trait("Category", TestCategories.Unit)]
    public class ViewNotFoundExceptionTests
    {
        [FactWithName]
        public void IsBaseException()
        {
            var ex = new ViewNotFoundException("a", "b");
            Assert.IsAssignableFrom<BaseCaughtException>(ex);
        }

        [FactWithName]
        public void ErrorMessageIsPopulated()
        {
            var ex = new ViewNotFoundException("sch", "vw");
            Assert.Equal("View 'sch.vw' not found.", ex.Message);
        }

        [FactWithName]
        public void HttpErrorMessageIsPopulated()
        {
            var ex = new ViewNotFoundException("sch", "vw");
            Assert.Equal("ViewNotFound", ex.HttpErrorMessage);
        }

        [FactWithName]
        public void HttpStatusCodeIsPopulated()
        {
            var ex = new ViewNotFoundException("sch", "vw");
            Assert.Equal(HttpStatusCode.InternalServerError, ex.ResponseCode);
        }
    }
}