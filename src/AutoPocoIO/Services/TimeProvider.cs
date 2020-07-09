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
        public DateTime LocalToday
        {
            get
            {
                var offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
                return DateTime.Today - offset;
            }
        }
    }
}
