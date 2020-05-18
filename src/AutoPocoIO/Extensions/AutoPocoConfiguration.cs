namespace AutoPocoIO.Extensions
{
    public static class AutoPocoConfiguration
    {
        public static string SaltVector { get; set; }
        public static string SecretKey { get; set; }

        public static string DashboardPathPrefix { get; set; }
        public static bool IsUsingEncryption { get { return !string.IsNullOrEmpty(SaltVector) && !string.IsNullOrEmpty(SecretKey); } }
        public static int? CacheTimeoutMinutes { get; set; }
    }
}
