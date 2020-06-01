using AutoPocoIO.Exceptions;
using System;
using System.Net;
using Xunit;

namespace AutoPocoIO.test.Exceptions
{
    [Trait("Category", TestCategories.Unit)]
    public class NoPrimaryKeyFoundExceptionTests
    {
        private class TestEx : NoPrimaryKeyFoundException
        {
            public TestEx() : base(null, new System.Runtime.Serialization.StreamingContext())
            {

            }
        }

        [FactWithName]
        public void IsBaseException()
        {
            var ex = new NoPrimaryKeyFoundException("a");
            Assert.IsAssignableFrom<BaseCaughtException>(ex);
        }

        [FactWithName]
        public void ErrorMessageIsPopulated()
        {
            var ex = new NoPrimaryKeyFoundException("entity");
            Assert.Equal("Entity 'entity' does not contain a primary key.", ex.Message);
        }

        [FactWithName]
        public void HttpErrorMessageIsPopulated()
        {
            var ex = new NoPrimaryKeyFoundException("entity");
            Assert.Equal("EntityKeyNotFound", ex.HttpErrorMessage);
        }

        [FactWithName]
        public void HttpStatusCodeIsPopulated()
        {
            var ex = new NoPrimaryKeyFoundException("entity");
            Assert.Equal(HttpStatusCode.InternalServerError, ex.ResponseCode);
        }

        [FactWithName]
        public void SerilizationConstructorNotImplmented()
        {
             void act() => new TestEx();
            Assert.Throws<NotImplementedException>(act);
        }
    }
}
