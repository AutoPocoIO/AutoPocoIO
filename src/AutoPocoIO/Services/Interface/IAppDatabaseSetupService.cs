using AutoPocoIO.DynamicSchema.Enums;

namespace AutoPocoIO.Services
{
    public interface IAppDatabaseSetupService
    {
        void Migrate();
        void SetupEncryption(string encryptionSalt, string encryptionSecretKey, int cacheTimeoutMinutes);
    }
}