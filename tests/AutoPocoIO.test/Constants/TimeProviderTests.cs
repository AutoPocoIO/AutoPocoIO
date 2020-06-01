using AutoPocoIO.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace AutoPocoIO.test.Constants
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class TimeProviderTests
    {
        [TestMethod]
        public void DefaultTimeProviderShowsUTCNow()
        {
            var now = new DefaultTimeProvider();
            //Check if now is within 5 seconds becuase its time
            TimeSpan time = DateTime.UtcNow - now.UtcNow;
            Assert.IsTrue(time < TimeSpan.FromSeconds(5));
        }

        [TestMethod]
        public void TimeProviderSetsTime()
        {
            var now = new DateTime(2011, 1, 1);
            var provider = new Mock<ITimeProvider>();
            provider.Setup(c => c.UtcNow).Returns(now);

            Assert.AreEqual(now, provider.Object.UtcNow);
        }
    }
}
