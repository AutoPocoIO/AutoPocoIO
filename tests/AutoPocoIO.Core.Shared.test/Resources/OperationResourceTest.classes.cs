using AutoPocoIO.DynamicSchema.Db;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.Models;
using AutoPocoIO.Resources;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;

namespace AutoPocoIO.test.Resources
{
    // Private classes used in this set of tests.  Seperated for clarity
    public partial class OperationResourceTests
    {
        private class ViewModel1
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Name2 { get; set; }
        }

        private class ViewModel2
        {
            public string Id1 { get; set; }
            public Guid Id2 { get; set; }
            public string Name3 { get; set; }
            public string Name4 { get; set; }
        }

        private class ViewModel3
        {
            public string Id2 { get; set; }
            public string Name5 { get; set; }
            public ViewModel1 ViewModel1 { get; set; }
        }

        private class ViewModel4
        {
            public int Id2 { get; set; }
            public string Name5 { get; set; }
            public IEnumerable<ViewModel5> UJ_pkJoinListFromother { get; set; }
        }

        private class ViewModel5
        {
            public int Other { get; set; }
        }

        private class TestResourceServices : OperationResource
        {

            public TestResourceServices(IServiceProvider serviceProvider) : base(serviceProvider) { }

            public override string ResourceType => "None";

            public IServiceProvider ExposeProvider { get => base.InternalServiceProvider; }
            public void SetConnector(Connector connector) => this.Connector = connector;

            public override void ApplyServices(IServiceCollection service, IServiceProvider rootProvider)
            {
                service.AddSingleton(c => new Mock<IDbSchemaBuilder>().Object);
                base.ApplyServices(service, rootProvider);
            }

            public override IDictionary<string, object> ExecuteProc(IDictionary<string, object> parameterDictionary)
            {
                throw new NotImplementedException();
            }
        }

        private class OtherConfig : Config
        { }
    }
}
