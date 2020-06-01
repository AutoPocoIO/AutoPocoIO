using AutoPocoIO.Exceptions;
using System.Net;
using Xunit;

namespace AutoPocoIO.test.Exceptions
{
    
     [Trait("Category", TestCategories.Unit)]
    public class TableNotFoundExceptionTests
    {
        [FactWithName]
        public void IsBaseException()
        {
            var ex = new TableNotFoundException("db", "a", "b");
            Assert.IsAssignableFrom<BaseCaughtException>(ex);
        }

        [FactWithName]
        public void ErrorMessageIsPopulated()
        {
            var ex = new TableNotFoundException("db", "sch", "tbl");
            Assert.Equal("Table 'db.sch.tbl' not found.", ex.Message);
        }

        [FactWithName]
        public void HttpErrorMessageIsPopulated()
        {
            var ex = new TableNotFoundException("db", "sch", "tbl");
            Assert.Equal("TableNotFound", ex.HttpErrorMessage);
        }

        [FactWithName]
        public void HttpStatusCodeIsPopulated()
        {
            var ex = new TableNotFoundException("db", "sch", "tbl");
            Assert.Equal(HttpStatusCode.InternalServerError, ex.ResponseCode);
        }
    }
}