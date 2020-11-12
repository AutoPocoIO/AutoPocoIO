using AutoPocoIO.Services;
using Moq;

namespace AutoPocoIO.test.WebApi
{
    public abstract class WebApiTestBase<TOp> where TOp : class
    {
        protected readonly ILoggingService LoggingService = Mock.Of<ILoggingService>();
        protected readonly Mock<TOp> Ops = new Mock<TOp>();
    }
}
