using System;

namespace AutoPocoIO.Services
{
    /// <summary>
    /// Get server and UTC date times.
    /// </summary>
    public interface ITimeProvider
    {
        /// <summary>
        /// UTC date and time now
        /// </summary>
        DateTime UtcNow { get; }
        /// <summary>
        /// Server date and time now
        /// </summary>
        DateTime Now { get; }
        /// <summary>
        /// Server date time at 00:00:00 UTC
        /// </summary>
        DateTime LocalToday { get; }
    }
}