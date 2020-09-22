using AutoPocoIO.Services;

namespace AutoPocoIO.Models
{
    /// <summary>
    /// Middileware set up options
    /// </summary>
    public class AutoPocoOptions
    {
        /// <summary>
        /// Default set up.
        /// UseDashboard = true.
        /// DashboardPath = "/autopoco".
        /// </summary>
        public AutoPocoOptions()
        {
            UseDashboard = true;
            DashboardPath = "/autopoco";
            CacheTimeoutMinutes = 240;
        }

        /// <summary>
        /// Route for dashboard.  DEfaults to /autopoco
        /// </summary>
        public string DashboardPath { get; set; }

        /// <summary>
        /// Flag to enable dashboard middleware. True by defaut.
        /// </summary>
        public bool UseDashboard { get; set; }
        public string SaltVector { get; set; }
        public string SecretKey { get; set; }
        public int CacheTimeoutMinutes { get; set; }
    }
}
