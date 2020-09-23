using System;

namespace AutoPocoIO.Services
{
    /// <summary>
    /// Get server and UTC date times.
    /// </summary>
    public class DefaultTimeProvider : ITimeProvider
    {
        /// <inheritdoc />
        public DateTime UtcNow => DateTime.UtcNow;
        /// <inheritdoc />
        public DateTime Now => DateTime.Now;
        /// <inheritdoc />
        public DateTime LocalToday => DateTime.Today - UtcOffset;
        /// <inheritdoc />
        public TimeSpan UtcOffset => TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
    }
}
