using AutoPocoIO.Dashboard.Pages;
using AutoPocoIO.Dashboard.Repos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AutoPocoIO.test.Dashboard.Pages
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class LayoutPageTests
    {
        [TestMethod]
        public void GetConnectorCount()
        {
            var repo = new Mock<IConnectorRepo>();
            repo.Setup(c => c.ConnectorCount()).Returns(123);

            var page = new Layout(repo.Object);
            var results = page.ConnectorCount();

            Assert.AreEqual(123, results);
        }
    }
}
