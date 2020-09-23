namespace AutoPocoIO.Services
{
    /// <summary>
    /// Set up connection string encyption and migrate database
    /// </summary>
    public interface IAppDatabaseSetupService
    {
        /// <summary>
        /// Initiate database migration.
        /// </summary>
        void Migrate();
        /// <summary>
        /// Set the encryption settings
        /// </summary>
        /// <param name="encryptionSalt">Must be length 16</param>
        /// <param name="encryptionSecretKey">Must be 128 characters</param>
        /// <param name="cacheTimeoutMinutes">Length in minutes how server values stay in cache.</param>
        /// <exception cref="System.ArgumentOutOfRangeException"/>
        void SetupEncryption(string encryptionSalt, string encryptionSecretKey, int cacheTimeoutMinutes);
    }
}