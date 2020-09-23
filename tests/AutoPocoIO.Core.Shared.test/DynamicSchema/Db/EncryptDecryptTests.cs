using AutoPocoIO.DynamicSchema.Util;
using AutoPocoIO.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutoPocoIO.test.DynamicSchema.Db
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class EncryptDecryptTests
    {
        private const string IvSalt = "49OQNVKPAWTMC747";
        private const string SecretKey = "401b09eab3c013d4ca54922bb802bec8fd5318192b0a75f201d8b3727429090fb337591abd3e44453b954555b7a0812e1081c39b740293f765eae731f5a65ed1";

        [TestInitialize]
        [TestCleanup]
        public void Init()
        {
            AutoPocoConfiguration.SaltVector = "";
            AutoPocoConfiguration.SecretKey = "";
        }

        [TestMethod]
        public void ReturnPlainTextIfNotConfigured()
        {
            var result = EncryptDecrypt.EncryptString("clearText");
            Assert.AreEqual("clearText", result);
        }


        [TestMethod]
        public void ReturnEncryptedTextIfConfigured()
        {
            AutoPocoConfiguration.SaltVector = IvSalt;
            AutoPocoConfiguration.SecretKey = SecretKey;

            var result = EncryptDecrypt.EncryptString("clearText");
            Assert.AreEqual("uK7lPbUa8xH+IPA5PLe4CQ==", result);
        }

        [TestMethod]
        public void ReturnEncryptedTextIfNotConfigured()
        {
            var result = EncryptDecrypt.DecryptString("stayEncrypted");
            Assert.AreEqual("stayEncrypted", result);
        }

        [TestMethod]
        public void ReturnDecryptedTextIfConfigured()
        {
            AutoPocoConfiguration.SaltVector = IvSalt;
            AutoPocoConfiguration.SecretKey = SecretKey;

            var result = EncryptDecrypt.DecryptString("uK7lPbUa8xH+IPA5PLe4CQ==");
            Assert.AreEqual("clearText", result);
        }
    }
}
