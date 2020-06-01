using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.Extensions;
using AutoPocoIO.Services;
using Xunit;
using Moq;
using System;
using System.Web.Http.Dependencies;

namespace AutoPocoIO.AspNet.test.Extensions
{
    
    [Trait("Category", TestCategories.Unit)]
    public class DependencyResolverExtensionTests
    {
        //[FactWithName]
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

        //[FactWithName]
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

        [FactWithName]
        public void GetRequiredServiceThrowsExceptionIfNotFound()
        {
            var dependencyResolver = new Mock<IDependencyResolver>();
            void act() => dependencyResolver.Object.GetRequiredService<IAppDatabaseSetupService>();
            Assert.Throws<ArgumentNullException>(act);
        }

        [FactWithName]
        public void GetRequiredServiceReturnsTypeIfFound()
        {
            var dbSetup = new Mock<IAppDatabaseSetupService>();
            var dependencyResolver = new Mock<IDependencyResolver>();
            dependencyResolver.Setup(c => c.GetService(typeof(IAppDatabaseSetupService)))
              .Returns(dbSetup.Object);

            var result = dependencyResolver.Object.GetRequiredService<IAppDatabaseSetupService>();
            Assert.Equal(dbSetup.Object, result);
        }

        //[FactWithName]
        //[ExpectedException(typeof(ArgumentNullException))]
        //public void CheckDepResolverNotNullForUseSqlEncryption()
        //{
        //    IDependencyResolver dependencyResolver = null;
        //    dependencyResolver.UseSqlServer("slt", "key", 123);
        //}

        //[FactWithName]
        //[ExpectedException(typeof(ArgumentNullException))]
        //public void CheckSaltNotNullForUseSqlEncryption()
        //{
        //    var dependencyResolver = new Mock<IDependencyResolver>();
        //    dependencyResolver.Object.UseSqlServer(null, "key", 123);
        //}

        //[FactWithName]
        //[ExpectedException(typeof(ArgumentNullException))]
        //public void CheckSecretKeyNotNullForUseSqlEncryption()
        //{
        //    var dependencyResolver = new Mock<IDependencyResolver>();
        //    dependencyResolver.Object.UseSqlServer("slt", null, 123);
        //}

        //[FactWithName]
        //[ExpectedException(typeof(ArgumentNullException))]
        //public void CheckDepResolverNotNullForUseSql()
        //{
        //    IDependencyResolver dependencyResolver = null;
        //    dependencyResolver.UseSqlServer();
        //}

        [FactWithName]
        public void CheckDepResolverNotNullForGetRequiredService()
        {
            IDependencyResolver dependencyResolver = null;
            void act() => dependencyResolver.GetRequiredService<string>();
            Assert.Throws<ArgumentNullException>(act);
        }
    }
}
