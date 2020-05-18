using AutoPocoIO.DynamicSchema.Db;
using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.Models;
using AutoPocoIO.Resources;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;

namespace AutoPocoIO.test.Resources
{
    // Private classes used in this set of tests.  Seperated for clarity
    public partial class OperationResourceTests
    {
        private class ViewModel1
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Name2 { get; set; }
        }

        private class ViewModel2
        {
            public int Id1 { get; set; }
            public string Name3 { get; set; }
            public string Name4 { get; set; }
        }

        private class TestResourceServices : OperationResource
        {

            public TestResourceServices(IServiceProvider serviceProvider) : base(serviceProvider) { }

            public override ResourceType ResourceType => ResourceType.None;

            public IServiceProvider ExposeProvider { get => base.InternalServiceProvider; }
            public void SetConnector(Connector connector) => this.Connector = connector;

            public override void ApplyServices(IServiceCollection service, IServiceProvider rootProvider)
            {
                service.AddSingleton(c => new Mock<IDbSchemaBuilder>().Object);
                base.ApplyServices(service, rootProvider);
            }
        }

        private class OtherConfig : Config
        { }
    }
}
