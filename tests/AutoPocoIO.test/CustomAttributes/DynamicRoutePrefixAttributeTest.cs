using AutoPocoIO.CustomAttributes;
using AutoPocoIO.Extensions;
using System;
using Xunit;

namespace AutoPocoIO.test.CustomAttributes
{
    
     [Trait("Category", TestCategories.Unit)]
    public class DynamicRoutePrefixAttributeTest : IDisposable
    {
        [FactWithName]
        public void DynamicRouteWithPrefix()
        {
            AutoPocoConfiguration.DashboardPathPrefix = "testPrefix";
            var attr = new DynamicRoutePrefixAttribute("test");
            Assert.Equal("testPrefix/test", attr.Prefix);
        }

        [FactWithName]
        public void DynamicRouteWithoutPrefix()
        {
            var attr = new DynamicRoutePrefixAttribute("test");
            Assert.Equal("test", attr.Prefix);
        }

        public void Dispose()
        {
            AutoPocoConfiguration.DashboardPathPrefix = null;
        }
    }
}
