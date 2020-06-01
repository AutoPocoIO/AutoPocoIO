using AutoPocoIO.DynamicSchema.Util;
using AutoPocoIO.Extensions;
using System;
using Xunit;

namespace AutoPocoIO.test.DynamicSchema.Db
{
    
     [Trait("Category", TestCategories.Unit)]
    public class EncryptDecryptTests : IDisposable
    {
        private const string IvSalt = "49OQNVKPAWTMC747";
        private const string SecretKey = "401b09eab3c013d4ca54922bb802bec8fd5318192b0a75f201d8b3727429090fb337591abd3e44453b954555b7a0812e1081c39b740293f765eae731f5a65ed1";

        public EncryptDecryptTests()
        {
            AutoPocoConfiguration.SaltVector = "";
            AutoPocoConfiguration.SecretKey = "";
        }

        [FactWithName]
        public void ReturnPlainTextIfNotConfigured()
        {
            var result = EncryptDecrypt.EncryptString("clearText");
            Assert.Equal("clearText", result);
        }


        [FactWithName]
        public void ReturnEncryptedTextIfConfigured()
        {
            AutoPocoConfiguration.SaltVector = IvSalt;
            AutoPocoConfiguration.SecretKey = SecretKey;

            var result = EncryptDecrypt.EncryptString("clearText");
            Assert.Equal("uK7lPbUa8xH+IPA5PLe4CQ==", result);
        }

        [FactWithName]
        public void ReturnEncryptedTextIfNotConfigured()
        {
            var result = EncryptDecrypt.DecryptString("stayEncrypted");
            Assert.Equal("stayEncrypted", result);
        }

        [FactWithName]
        public void ReturnDecryptedTextIfConfigured()
        {
            AutoPocoConfiguration.SaltVector = IvSalt;
            AutoPocoConfiguration.SecretKey = SecretKey;

            var result = EncryptDecrypt.DecryptString("uK7lPbUa8xH+IPA5PLe4CQ==");
            Assert.Equal("clearText", result);
        }

        public void Dispose()
        {
            AutoPocoConfiguration.SaltVector = "";
            AutoPocoConfiguration.SecretKey = "";
        }
    }
}
