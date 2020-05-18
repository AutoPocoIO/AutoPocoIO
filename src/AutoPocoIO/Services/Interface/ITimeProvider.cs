using System;

namespace AutoPocoIO.Services
{
    public interface ITimeProvider
    {
        DateTime UtcNow { get; }
        DateTime Now { get; }

        DateTime LocalToday { get; }
    }
}