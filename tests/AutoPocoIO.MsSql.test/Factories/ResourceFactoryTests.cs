﻿using AutoPocoIO.DynamicSchema.Db;
using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.Factories;
using AutoPocoIO.Models;
using AutoPocoIO.Resources;
using AutoPocoIO.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AutoPocoIO.MsSql.test.Factories
{
    
    [Trait("Category", TestCategories.Unit)]
    public class ResourceFactoryTests
    {
        private IResourceFactory _resourceFactory;
        public void Init(int resouceType)
        {
            var appAdminService = new Mock<IAppAdminService>();
            appAdminService.Setup(c => c.GetConnection("conn1"))
                .Returns(new Connector
                {
                    Id = 1,
                    Name = "conn1",
                    ResourceType = resouceType,
                    ConnectionStringDecrypted = "connStr1"

                });

            appAdminService.Setup(c => c.GetConnection(1))
              .Returns(new Connector
              {
                  Id = 1,
                  Name = "conn1",
                  ResourceType = resouceType,
                  ConnectionStringDecrypted = "connStr1"

              });

            var services = new ServiceCollection()
                .AddSingleton(appAdminService.Object)
                .BuildServiceProvider();

            var connStringFactory = new Mock<IConnectionStringFactory>();
            connStringFactory.Setup(c => c.GetConnectionInformation(resouceType, "connStr1"))
                .Returns(new ConnectionInformation());

            var resourceServices = new ServiceCollection()
                .AddSingleton(new Config())
                .AddTransient(c => connStringFactory.Object)
                .AddSingleton<ISchemaInitializer>(new SchemaInitializer(new Config(), Mock.Of<IDbSchemaBuilder>()))
                .BuildServiceProvider();


            PrivateObject authProvider = new PrivateObject(ServiceProviderCache.Instance);
            var dictionary = (ConcurrentDictionary<ResourceType, IServiceProvider>)authProvider.GetField("_configurations");
            dictionary.Clear();
            dictionary.GetOrAdd(ResourceType.Mssql, resourceServices);

            var list = new List<IOperationResource> { new MsSqlResource(resourceServices) };

            _resourceFactory = new ResourceFactory(services, list);
        }
        [FactWithName]
        public void GetSqlResource()
        {
            Init(1);
            var resource = _resourceFactory.GetResource("conn1", OperationType.read, "obj1");
            Assert.IsType<MsSqlResource>(resource);
        }
    }
}