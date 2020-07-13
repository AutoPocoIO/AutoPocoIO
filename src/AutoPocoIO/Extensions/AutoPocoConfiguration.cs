namespace AutoPocoIO.Extensions
{
    /// <summary>
    /// Configuration values
    /// </summary>
    public static class AutoPocoConfiguration
    {
        /// <summary>
        /// Get or set encryption salt.
        /// </summary>
        public static string SaltVector { get; set; }
        /// <summary>
        /// Get or set encryption secret.
        /// </summary>
        public static string SecretKey { get; set; }
        /// <summary>
        /// Get or set dashboard url prefix.
        /// </summary>
        public static string DashboardPathPrefix { get; set; }
        /// <summary>
        /// Flag showing if both salt and secret key have values.
        /// </summary>
        public static bool IsUsingEncryption { get { return !string.IsNullOrEmpty(SaltVector) && !string.IsNullOrEmpty(SecretKey); } }
        /// <summary>
        /// Get or set server cache timeout.
        /// </summary>
        public static int? CacheTimeoutMinutes { get; set; }
    }
}
