using AutoPocoIO.Exceptions;
using System.Net;
using Xunit;

namespace AutoPocoIO.test.Exceptions
{
    [Trait("Category", TestCategories.Unit)]
    public class StoreProcedureNotFoundExceptionTests
    {
        [FactWithName]
        public void IsBaseException()
        {
            var ex = new StoreProcedureNotFoundException("a", "b");
            Assert.IsAssignableFrom<BaseCaughtException>(ex);
        }

        [FactWithName]
        public void ErrorMessageIsPopulated()
        {
            var ex = new StoreProcedureNotFoundException("sch", "sp");
            Assert.Equal("Stored Procedure 'sch.sp' not found.", ex.Message);
        }

        [FactWithName]
        public void HttpErrorMessageIsPopulated()
        {
            var ex = new StoreProcedureNotFoundException("sch", "sp");
            Assert.Equal("StoreProcedureNotFound", ex.HttpErrorMessage);
        }

        [FactWithName]
        public void HttpStatusCodeIsPopulated()
        {
            var ex = new StoreProcedureNotFoundException("sch", "sp");
            Assert.Equal(HttpStatusCode.InternalServerError, ex.ResponseCode);
        }
    }
}