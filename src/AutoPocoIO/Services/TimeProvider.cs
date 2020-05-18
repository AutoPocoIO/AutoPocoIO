using System;

namespace AutoPocoIO.Services
{
    public class DefaultTimeProvider : ITimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
        public DateTime Now => DateTime.Now;

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
