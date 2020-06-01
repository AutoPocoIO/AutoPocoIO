using AutoPocoIO.Exceptions;
using System;
using Xunit;

namespace AutoPocoIO.test.Exceptions
{
    [Trait("Category", TestCategories.Unit)]
    public class CheckTests
    {
        [FactWithName]
        public void NotNullNoException()
        {
            var a = "asdfasdf";
            Check.NotNull(a, "a");
        }

        [FactWithName]
        public void NotNullNullThrowsException()
        {
            string a = null;
             void act() => Check.NotNull(a, "a");
            Assert.Throws<ArgumentNullException>(act);
        }

        [FactWithName]
        public void NotEmptyNoExceptionIfValue()
        {
            var a = "asdfasdf";
            var result = Check.NotEmpty(a, "a");
            Assert.Equal(a, result);
        }

        [FactWithName]
        public void NotEmptyNoExceptionIfNull()
        {
            string a = null;
             void act() => Check.NotEmpty(a, "a");
            Assert.Throws<ArgumentNullException>(act);
        }

        [FactWithName]
        public void NotEmptyNoExceptionIfEmptyOrWhiteSpace()
        {
            string a = "     ";
             void act() => Check.NotEmpty(a, "a");
            Assert.Throws<ArgumentException>(act);
        }

        [FactWithName]
        public void NotEmptyThrowsExceptionIfParamNull()
        {
            string a = null;
             void act() => Check.NotEmpty(a, null);
            Assert.Throws<ArgumentNullException>(act);
        }

        [FactWithName]
        public void NotEmptyThrowsExceptionIfParamEmpty()
        {
            string a = null;
             void act() => Check.NotEmpty(a, "");
            Assert.Throws<ArgumentException>(act);
        }
    }
}
