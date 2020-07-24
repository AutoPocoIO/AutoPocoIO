using AutoPocoIO.Context;
using AutoPocoIO.DynamicSchema.Db;
using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.DynamicSchema.Util;
using AutoPocoIO.Factories;
using AutoPocoIO.Models;
using AutoPocoIO.Resources;
using AutoPocoIO.test;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace AutoPocoIO.MsSql.test.Resources
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class MsSqlResourceTests
    {
        Mock<IDbCommand> command;
        List<DbParameter> usedParams;

        [TestInitialize]
        public void Init()
        {

            var conn = new Mock<IDbConnection>();
            conn.Setup(c => c.Open()).Verifiable();

            usedParams = new List<DbParameter>();

            var dbParams = new Mock<IDataParameterCollection>();
            dbParams.Setup(c => c.Add(It.IsAny<object>()))
                .Callback<object>(c => usedParams.Add((DbParameter)c));

            command = new Mock<IDbCommand>();
            command.Setup(c => c.Connection).Returns(conn.Object);
            command.Setup(c => c.Parameters).Returns(dbParams.Object);

            var options = new DbContextOptionsBuilder<DbContextBase>()
                 .UseInMemoryDatabase(databaseName: "appDb" + Guid.NewGuid().ToString())
                 .Options;

            var dbAdapter = new Mock<IDbAdapter>();
            dbAdapter.Setup(c => c.CreateDbCommand()).Returns(command.Object);

            var config = new Config();
            var schema = new Mock<IDbSchema>();

            var inProc = new StoredProcedure { Schema = "sch1", Name = "Name2", Database = "dbo" };
            inProc.Parameters.Add(new DBParameter { Name = "param1", IsOutput = false, Type = "DateTime" });

            var outProc = new StoredProcedure { Schema = "sch1", Name = "Name3", Database = "dbo" };
            outProc.Parameters.Add(new DBParameter { Name = "param1", IsOutput = true, Type = "Int" });


            schema.Setup(c => c.StoredProcedures)
                .Returns(new List<StoredProcedure>
                {
                    new StoredProcedure { Schema = "sch1", Name = "Name1", Database = "dbo" },
                       inProc,
                       outProc
                });

            var connStringFactory = new Mock<IConnectionStringFactory>();
            connStringFactory.Setup(c => c.GetConnectionInformation(1, "conn1"))
                .Returns(new ConnectionInformation { InitialCatalog = "dbo" });

            var services = new ServiceCollection();
            services.AddSingleton(Mock.Of<IDbSchemaBuilder>());
            services.AddSingleton(connStringFactory.Object);
            services.AddSingleton(schema.Object);
            services.AddSingleton(dbAdapter.Object);
            services.AddSingleton(config);
            services.AddSingleton<ISchemaInitializer>(new SchemaInitializer(config, Mock.Of<IDbSchemaBuilder>(), schema.Object));

            var provider = services.BuildServiceProvider();

            PrivateObject authProvider = new PrivateObject(ServiceProviderCache.Instance);
            var dictionary = (ConcurrentDictionary<ResourceType, IServiceProvider>)authProvider.GetField("_configurations");
            dictionary.Clear();
            dictionary.GetOrAdd(ResourceType.Mssql, provider);

        }

        [TestMethod]
        public void ExecuteProcedureWithNoParams()
        {
            var reader = new Mock<IDataReader>();
            reader.SetupSequence(m => m.Read())
                      .Returns(false); // Done reading

            string commandText = "";
            var outputParams = new Dictionary<string, object>();
            command.SetupSet(c => c.CommandText = It.IsAny<string>())
                .Callback<string>(c => commandText = c);
            command.Setup(c => c.ExecuteReader()).Returns(reader.Object);

            //mock to allow for load to 
            var resource = new MsSqlResource(new ServiceCollection().BuildServiceProvider());
            resource.ConfigureAction(new Connector { ResourceType = 1, Schema = "sch1", ConnectionString = "conn1" }, OperationType.Any, "Name1");

            var results = resource.ExecuteProc(new Dictionary<string, object>());

            Assert.AreEqual("[dbo].[sch1].[Name1] ", commandText);
            Assert.AreEqual(0, usedParams.Count());
        }

        [TestMethod]
        public void ExecuteProcedureWithInputParams()
        {
            var reader = new Mock<IDataReader>();
            reader.SetupSequence(m => m.Read())
                      .Returns(false); // Done reading


            string commandText = "";
            var outputParams = new Dictionary<string, object>();
            command.SetupSet(c => c.CommandText = It.IsAny<string>())
                .Callback<string>(c => commandText = c);
            command.Setup(c => c.ExecuteReader()).Returns(reader.Object);

            var resource = new MsSqlResource(new ServiceCollection().BuildServiceProvider());
            resource.ConfigureAction(new Connector { ResourceType = 1, Schema = "sch1", ConnectionString = "conn1" }, OperationType.Any, "Name2");

            var results = (IDictionary<string, object>)resource.ExecuteProc(new Dictionary<string, object>());

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("[dbo].[sch1].[Name2] param1", commandText);
            Assert.AreEqual(1, usedParams.Count());
            Assert.AreEqual(ParameterDirection.Input, usedParams.First().Direction);
            Assert.AreEqual("param1", usedParams.First().ParameterName);
        }

        [TestMethod]
        public void ExecuteProcedureWithOutputParams()
        {
            var reader = new Mock<IDataReader>();
            reader.SetupSequence(m => m.Read())
                      .Returns(false); // Done reading


            string commandText = "";
            var outputParams = new Dictionary<string, object>();
            command.SetupSet(c => c.CommandText = It.IsAny<string>())
                .Callback<string>(c => commandText = c);
            command.Setup(c => c.ExecuteReader()).Returns(reader.Object);

            //mock to allow for load to 
            var resource = new MsSqlResource(new ServiceCollection().BuildServiceProvider());
            resource.ConfigureAction(new Connector { ResourceType = 1, Schema = "sch1", ConnectionString = "conn1" }, OperationType.Any, "Name3");

            var results = (IDictionary<string, object>)resource.ExecuteProc(new Dictionary<string, object>());

            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(DBNull.Value, results["param1"]);

            Assert.AreEqual("[dbo].[sch1].[Name3] param1 out", commandText);
            Assert.AreEqual(1, usedParams.Count());
            Assert.AreEqual(ParameterDirection.Output, usedParams.First().Direction);
            Assert.AreEqual("param1", usedParams.First().ParameterName);
        }

        [TestMethod]
        public void ExecuteProcedureWithOutputParamsPassValue()
        {
            var reader = new Mock<IDataReader>();
            reader.SetupSequence(m => m.Read())
                      .Returns(false); // Done reading


            string commandText = "";
            var outputParams = new Dictionary<string, object>();
            command.SetupSet(c => c.CommandText = It.IsAny<string>())
                .Callback<string>(c => commandText = c);
            command.Setup(c => c.ExecuteReader()).Returns(reader.Object);

            //mock to allow for load to 
            var resource = new MsSqlResource(new ServiceCollection().BuildServiceProvider());
            resource.ConfigureAction(new Connector { ResourceType = 1, Schema = "sch1", ConnectionString = "conn1" }, OperationType.Any, "Name3");

            var results = (IDictionary<string, object>)resource.ExecuteProc(new Dictionary<string, object>() { { "param1", 123 } });

            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(123, results["param1"]);

            Assert.AreEqual("[dbo].[sch1].[Name3] param1 out", commandText);
            Assert.AreEqual(1, usedParams.Count());
            Assert.AreEqual(ParameterDirection.Output, usedParams.First().Direction);
            Assert.AreEqual("param1", usedParams.First().ParameterName);
        }


        [TestMethod]
        public void ServicesRegistered()
        {
            var rootProvider = new ServiceCollection();
            rootProvider.AddSingleton(new DbContextOptions<AppDbContext>());

            var resource = new MsSqlResource(new ServiceCollection().BuildServiceProvider());
            var collection = new ServiceCollection();

            resource.ApplyServices(collection, rootProvider.BuildServiceProvider());

            var provider = collection.BuildServiceProvider();

            Assert.IsNotNull(provider.GetService<IDbSchemaBuilder>());
            Assert.IsNotNull(provider.GetService<MsSqlSchmeaQueries>());
            Assert.IsNotNull(provider.GetService<IDbTypeMapper>());
            Assert.IsNotNull(provider.GetService<IConnectionStringBuilder>());
        }
    }
}
