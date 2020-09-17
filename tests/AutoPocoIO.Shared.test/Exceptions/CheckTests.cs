using AutoPocoIO.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AutoPocoIO.test.Exceptions
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class CheckTests
    {
        [TestMethod]
        public void NotNullNoException()
        {
            var a = "asdfasdf";
            Check.NotNull(a, "a");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NotNullNullThrowsException()
        {
            string a = null;
            Check.NotNull(a, "a");
        }

        [TestMethod]
        public void NotEmptyNoExceptionIfValue()
        {
            var a = "asdfasdf";
            var result = Check.NotEmpty(a, "a");
            Assert.AreEqual(a, result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NotEmptyNoExceptionIfNull()
        {
            string a = null;
            Check.NotEmpty(a, "a");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void NotEmptyNoExceptionIfEmptyOrWhiteSpace()
        {
            string a = "     ";
            Check.NotEmpty(a, "a");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NotEmptyThrowsExceptionIfParamNull()
        {
            string a = null;
            Check.NotEmpty(a, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void NotEmptyThrowsExceptionIfParamEmpty()
        {
            string a = null;
            Check.NotEmpty(a, "");
        }
    }
}
