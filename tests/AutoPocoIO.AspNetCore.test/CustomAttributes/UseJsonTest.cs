using AutoPocoIO.CustomAttributes;
using Xunit;
using System.Linq;

namespace AutoPocoIO.AspNetCore.test.CustomAttributes
{
    
    public class UseJsonTest
    {
        [FactWithName]
        [Trait("Category", TestCategories.Unit)]
        public void VerifyProduces()
        {
            var attr = new UseJsonAttribute();

            Assert.Equal(2, attr.ContentTypes.Count);
            Assert.Equal("application/json", attr.ContentTypes.First());
            Assert.Equal("text/plain", attr.ContentTypes.Last());
        }
    }
}
