using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.Exceptions;
using AutoPocoIO.Extensions;
using Moq;
using System;
using System.Data.Common;
using Xunit;

namespace AutoPocoIO.test.Extensions
{
    [Trait("Category", TestCategories.Unit)]
    public class SchemaBuilderExtensionsTests
    {
        [FactWithName]
        public void OpenDbConnection()
        {
            var dbconnectionMock = new Mock<DbConnection>();
            dbconnectionMock.Setup(c => c.Open());

            dbconnectionMock.Object.SafeDbConnectionOpen("test");

            dbconnectionMock.Verify(c => c.Open(), Times.Once);
        }

        [FactWithName]
        public void OpenDbconnectionFail()
        {
            var mockException = new Mock<DbException>();
            var dbconnectionMock = new Mock<DbConnection>();
            dbconnectionMock.Setup(c => c.Open())
                            .Throws(mockException.Object);


             void act() => dbconnectionMock.Object.SafeDbConnectionOpen("test");
            Assert.Throws<OpenConnectorException>(act);
        }

        [FactWithName]
        public void TableIsU()
        {
            var tableType = "U".SetObjectType();
            Assert.Equal(DBOjectType.Table, tableType);
        }

        [FactWithName]
        public void ViewIsV()
        {
            var viewType = "V".SetObjectType();
            Assert.Equal(DBOjectType.View, viewType);
        }

        [FactWithName]
        public void ObjectTypeNotFound()
        {
             void act() => "notFound".SetObjectType();
            Assert.Throws<ArgumentOutOfRangeException>(act);
        }
    }
}
