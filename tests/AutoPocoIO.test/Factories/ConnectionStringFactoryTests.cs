using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.Factories;
using AutoPocoIO.Resources;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace AutoPocoIO.test.Factories
{
    [Trait("Category", TestCategories.Unit)]
    public class ConnectionStringFactoryTests
    {
        private readonly IConnectionStringFactory _resourceFactory;
        public ConnectionStringFactoryTests()
        {
            var builder = new Mock<IConnectionStringBuilder>();
            builder.Setup(c => c.ResourceType)
                .Returns(ResourceType.Mssql);

            builder.Setup(c => c.ParseConnectionString("parseABC"))
                .Returns(new ConnectionInformation { InitialCatalog = "cat1" });

            builder.Setup(c => c.CreateConnectionString(It.IsAny<ConnectionInformation>()))
                .Returns<ConnectionInformation>(c => "conn:" + c.InitialCatalog);

            var list = new List<IConnectionStringBuilder> { builder.Object };

            _resourceFactory = new ConnectionStringFactory(list);
        }

        [FactWithName]
        public void GetConnectionInfo()
        {
            var connInfo = _resourceFactory.GetConnectionInformation(1, "parseABC");
            Assert.Equal("cat1", connInfo.InitialCatalog);
        }

        [FactWithName]
        public void SetConnectionInfo()
        {
            var connInfo = new ConnectionInformation { InitialCatalog = "abc" };
            var connString = _resourceFactory.CreateConnectionString(1, connInfo);
            Assert.Equal("conn:abc", connString);
        }

        [FactWithName]
        public void GetNotRegisteredResource()
        {
             void act() => _resourceFactory.GetConnectionInformation(2, "parseABC");
            Assert.Throws<ArgumentException>(act);
        }

        [FactWithName]
        public void GetUnimplmentedResource()
        {
             void act() => _resourceFactory.GetConnectionInformation(2124, "parseABC");
            Assert.Throws<ArgumentOutOfRangeException>(act);
        }
    }
}