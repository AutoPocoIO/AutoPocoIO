using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.Exceptions;
using AutoPocoIO.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Data.Common;

namespace AutoPocoIO.test.Extensions
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class SchemaBuilderExtensionsTests
    {
        [TestMethod]
        public void OpenDbConnection()
        {
            var dbconnectionMock = new Mock<DbConnection>();
            dbconnectionMock.Setup(c => c.Open());

            dbconnectionMock.Object.SafeDbConnectionOpen("test");

            dbconnectionMock.Verify(c => c.Open(), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(OpenConnectorException))]
        public void OpenDbconnectionFail()
        {
            var mockException = new Mock<DbException>();
            var dbconnectionMock = new Mock<DbConnection>();
            dbconnectionMock.Setup(c => c.Open())
                            .Throws(mockException.Object);


            dbconnectionMock.Object.SafeDbConnectionOpen("test");
        }

        [TestMethod]
        public void TableIsU()
        {
            var tableType = "U".SetObjectType();
            Assert.AreEqual(DBOjectTypes.Table, tableType);
        }

        [TestMethod]
        public void ViewIsV()
        {
            var viewType = "V".SetObjectType();
            Assert.AreEqual(DBOjectTypes.View, viewType);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ObjectTypeNotFound()
        {
            "notFound".SetObjectType();
        }
    }
}
