using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.Factories;
using AutoPocoIO.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace AutoPocoIO.test.Factories
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class ConnectionStringFactoryTests
    {
        private IConnectionStringFactory _resourceFactory;

        [TestInitialize]
        public void Init()
        {
            var builder = new Mock<IConnectionStringBuilder>();
            builder.Setup(c => c.ResourceType)
                .Returns("type1");

            builder.Setup(c => c.ParseConnectionString("parseABC"))
                .Returns(new ConnectionInformation { InitialCatalog = "cat1" });

            builder.Setup(c => c.CreateConnectionString(It.IsAny<ConnectionInformation>()))
                .Returns<ConnectionInformation>(c => "conn:" + c.InitialCatalog);

            var list = new List<IConnectionStringBuilder> { builder.Object };

            _resourceFactory = new ConnectionStringFactory(list);
        }

        [TestMethod]
        public void GetConnectionInfo()
        {
            var connInfo = _resourceFactory.GetConnectionInformation("type1", "parseABC");
            Assert.AreEqual("cat1", connInfo.InitialCatalog);
        }

        [TestMethod]
        public void SetConnectionInfo()
        {
            var connInfo = new ConnectionInformation { InitialCatalog = "abc" };
            var connString = _resourceFactory.CreateConnectionString("type1", connInfo);
            Assert.AreEqual("conn:abc", connString);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetNotRegisteredResource()
        {
            _resourceFactory.GetConnectionInformation("type123", "parseABC");
        }
    }
}