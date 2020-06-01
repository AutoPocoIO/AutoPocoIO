using AutoPocoIO.Services;
using Xunit;
using Moq;
using System;

namespace AutoPocoIO.test.Constants
{
    
     [Trait("Category", TestCategories.Unit)]
    public class TimeProviderTests
    {
        [FactWithName]
        public void DefaultTimeProviderShowsUTCNow()
        {
            var now = new DefaultTimeProvider();
            //Check if now is within 5 seconds becuase its time
            TimeSpan time = DateTime.UtcNow - now.UtcNow;
             Assert.True(time < TimeSpan.FromSeconds(5));
        }

        [FactWithName]
        public void TimeProviderSetsTime()
        {
            var now = new DateTime(2011, 1, 1);
            var provider = new Mock<ITimeProvider>();
            provider.Setup(c => c.UtcNow).Returns(now);

            Assert.Equal(now, provider.Object.UtcNow);
        }
    }
}
