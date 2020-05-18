using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.Extensions;
using AutoPocoIO.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Web.Http.Dependencies;

namespace AutoPocoIO.AspNet.test.Extensions
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class DependencyResolverExtensionTests
    {
        //[TestMethod]
        //public void UseSqlServerWithEncryption()
        //{
        //    var dbSetup = new Mock<IAppDatabaseSetupService>();
        //    dbSetup.Setup(c => c.SetupEncryption("slt", "key", 123)).Verifiable();
        //    dbSetup.Setup(c => c.Migrate(ResourceType.Mssql)).Verifiable();

        //    var dependencyResolver = new Mock<IDependencyResolver>();
        //    dependencyResolver.Setup(c => c.GetService(typeof(IAppDatabaseSetupService)))
        //        .Returns(dbSetup.Object);

        //    dependencyResolver.Object.UseSqlServer("slt", "key", 123);

        //    dbSetup.Verify();
        //}

        //[TestMethod]
        //public void UseSqlServerWithoutEncryption()
        //{
        //    var dbSetup = new Mock<IAppDatabaseSetupService>();

        //    dbSetup.Setup(c => c.Migrate(ResourceType.Mssql)).Verifiable();

        //    var dependencyResolver = new Mock<IDependencyResolver>();
        //    dependencyResolver.Setup(c => c.GetService(typeof(IAppDatabaseSetupService)))
        //        .Returns(dbSetup.Object);

        //    dependencyResolver.Object.UseSqlServer();

        //    dbSetup.Verify();
        //    dbSetup.Verify(c => c.SetupEncryption(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        //}

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetRequiredServiceThrowsExceptionIfNotFound()
        {
            var dependencyResolver = new Mock<IDependencyResolver>();
            dependencyResolver.Object.GetRequiredService<IAppDatabaseSetupService>();
        }

        [TestMethod]
        public void GetRequiredServiceReturnsTypeIfFound()
        {
            var dbSetup = new Mock<IAppDatabaseSetupService>();
            var dependencyResolver = new Mock<IDependencyResolver>();
            dependencyResolver.Setup(c => c.GetService(typeof(IAppDatabaseSetupService)))
              .Returns(dbSetup.Object);

            var result = dependencyResolver.Object.GetRequiredService<IAppDatabaseSetupService>();
            Assert.AreEqual(dbSetup.Object, result);
        }

        //[TestMethod]
        //[ExpectedException(typeof(ArgumentNullException))]
        //public void CheckDepResolverNotNullForUseSqlEncryption()
        //{
        //    IDependencyResolver dependencyResolver = null;
        //    dependencyResolver.UseSqlServer("slt", "key", 123);
        //}

        //[TestMethod]
        //[ExpectedException(typeof(ArgumentNullException))]
        //public void CheckSaltNotNullForUseSqlEncryption()
        //{
        //    var dependencyResolver = new Mock<IDependencyResolver>();
        //    dependencyResolver.Object.UseSqlServer(null, "key", 123);
        //}

        //[TestMethod]
        //[ExpectedException(typeof(ArgumentNullException))]
        //public void CheckSecretKeyNotNullForUseSqlEncryption()
        //{
        //    var dependencyResolver = new Mock<IDependencyResolver>();
        //    dependencyResolver.Object.UseSqlServer("slt", null, 123);
        //}

        //[TestMethod]
        //[ExpectedException(typeof(ArgumentNullException))]
        //public void CheckDepResolverNotNullForUseSql()
        //{
        //    IDependencyResolver dependencyResolver = null;
        //    dependencyResolver.UseSqlServer();
        //}

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CheckDepResolverNotNullForGetRequiredService()
        {
            IDependencyResolver dependencyResolver = null;
            dependencyResolver.GetRequiredService<string>();
        }
    }
}
