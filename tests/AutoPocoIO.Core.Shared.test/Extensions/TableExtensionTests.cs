using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.Exceptions;
using AutoPocoIO.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AutoPocoIO.test.Extensions
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class TableExtensionTests
    {
        [TestMethod]
        public void KeyTypeMatches()
        {
            var pks = new[]
            {
                new PrimaryKeyInformation
                { 
                    Name = "pk1",
                    Type = typeof(int)
                }
            };

            var keys = new object[]
            {
                1
            };

            TableExtensions.GetTableKeys(pks, keys);

            Assert.AreEqual(1, pks[0].Value);
        }

        [TestMethod]
        public void ConvertKeyTypeMatches()
        {
            var pks = new[]
            {
                new PrimaryKeyInformation
                {
                    Name = "pk1",
                    Type = typeof(int)
                }
            };

            var keys = new object[]
            {
                "1"
            };

            TableExtensions.GetTableKeys(pks, keys);

            Assert.AreEqual(1, pks[0].Value);
        }

        [TestMethod]
        [ExpectedException(typeof(PrimaryKeyTypeMismatchException))]
        public void KeyTypeMismatch()
        {
            var pks = new[]
           {
                new PrimaryKeyInformation
                {
                    Name = "pk1",
                    Type = typeof(int)
                }
            };

            var keys = new object[]
            {
                Guid.NewGuid()
            };

            TableExtensions.GetTableKeys(pks, keys);
        }

        [TestMethod]
        [ExpectedException(typeof(PrimaryKeyTypeMismatchException))]
        public void ConvertKeyTypeMismatch()
        {
            var pks = new[]
           {
                new PrimaryKeyInformation
                {
                    Name = "pk1",
                    Type = typeof(int)
                }
            };

            var keys = new object[]
            {
                "abc"
            };

            TableExtensions.GetTableKeys(pks, keys);
        }

    }
}
