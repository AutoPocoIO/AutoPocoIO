using AutoPocoIO.Context;
using AutoPocoIO.DynamicSchema.Db;
using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.DynamicSchema.Runtime;
using AutoPocoIO.DynamicSchema.Util;
using AutoPocoIO.Factories;
using AutoPocoIO.Models;
using AutoPocoIO.Resources;
using AutoPocoIO.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace AutoPocoIO.test.Resources
{
    
    [Trait("Category", TestCategories.Unit)]
    public partial class OperationResourceTests
    {
        private readonly DbContextOptions<AppDbContext> appDbOptions;
        private readonly ServiceCollection defaultServices;
        private readonly Config config;
        private readonly Mock<ISchemaInitializer> schemaInitializer;
        private readonly Connector defaultConnector;

        private void ResetServiceProviderCache(IServiceCollection services = null)
        {
            if (services == null)
                services = defaultServices;

            PrivateObject authProvider = new PrivateObject(ServiceProviderCache.Instance);
            var dictionary = (ConcurrentDictionary<ResourceType, IServiceProvider>)authProvider.GetField("_configurations");
            dictionary.Clear();
            dictionary.GetOrAdd(ResourceType.None, services.BuildServiceProvider());
        }

        private void ClearServiceProviderCache()
        {
            PrivateObject authProvider = new PrivateObject(ServiceProviderCache.Instance);
            var dictionary = (ConcurrentDictionary<ResourceType, IServiceProvider>)authProvider.GetField("_configurations");
            dictionary.Clear();
        }

        public OperationResourceTests()
        {
            config = new Config();
            schemaInitializer = new Mock<ISchemaInitializer>();
            defaultConnector = new Connector { Id = 1, Name = "testConn", InitialCatalog = "db1", Schema = "sch", ResourceType = 43, ConnectionStringDecrypted = "conn" };
            appDbOptions = new DbContextOptionsBuilder<AppDbContext>()
              .UseInMemoryDatabase(databaseName: "appDb" + Guid.NewGuid().ToString())
              .Options;

            defaultServices = new ServiceCollection();
            defaultServices.AddSingleton(config);
            defaultServices.AddSingleton(new AppDbContext(appDbOptions));
            defaultServices.AddSingleton(appDbOptions);
            defaultServices.AddSingleton(Mock.Of<IDbSchemaBuilder>());
            defaultServices.AddSingleton(schemaInitializer.Object);
            defaultServices.AddSingleton(c =>
            {
                var mock = new Mock<IDbSchema>();
                mock.Setup(d => d.Tables).Returns(new List<Table>
                {
                    new Table{Database = "db1", Schema = "sch", Name = "tbl1"}
                });
                return mock.Object;
            });

            defaultServices.AddSingleton(c =>
            {
                var mock = new Mock<IConnectionStringFactory>();
                mock.Setup(d => d.GetConnectionInformation(43, "conn"))
                .Returns(new ConnectionInformation { UserId = "user1", InitialCatalog = "db1" });
                return mock.Object;
            });

            ResetServiceProviderCache();

        }

        [FactWithName]
        public void ConfirmInternalServicesAreRegistered()
        {
            ClearServiceProviderCache();
            var resource = new TestResourceServices(defaultServices.BuildServiceProvider());

             Assert.NotNull(resource.ExposeProvider.GetService<AppDbContext>());
             Assert.NotNull(resource.ExposeProvider.GetService<DynamicClassBuilder>());
             Assert.NotNull(resource.ExposeProvider.GetService<IDbAdapter>());
             Assert.NotNull(resource.ExposeProvider.GetService<Config>());
             Assert.NotNull(resource.ExposeProvider.GetService<IDbSchema>());
             Assert.NotNull(resource.ExposeProvider.GetService<IDbTypeMapper>());
             Assert.NotNull(resource.ExposeProvider.GetService<IConnectionStringFactory>());
        }

        [FactWithName]
        public void ConfgiureActionAssignsValues()
        {
            var resource = new TestResourceServices(defaultServices.BuildServiceProvider());

            var connector = new Connector()
            {
                ConnectionStringDecrypted = "conn",
                ResourceType = 43
            };

            resource.ConfigureAction(connector, OperationType.read, "obj1");

            Assert.Equal(connector, resource.Connector);
            Assert.Equal("user1", connector.UserId);
            Assert.Equal("obj1", resource.DbObjectName);

            schemaInitializer.Verify(c => c.ConfigureAction(connector, OperationType.read), Times.Once);
        }

        [FactWithName]
        public void GetAllSchemasFromDb()
        {
            using (var db = new AppDbContext(appDbOptions))
            {
                db.Connector.Add(new Connector
                {
                    InitialCatalog = "db1",
                    Schema = "sch1",
                    Id = 1,
                    Name = "conn1"
                });
                db.Connector.Add(new Connector
                {
                    InitialCatalog = "db1",
                    Schema = "asch1",
                    Id = 2,
                    Name = "conn2"
                });
                db.SaveChanges();
            }

            ResetServiceProviderCache();

            var resource = new TestResourceServices(defaultServices.BuildServiceProvider());
            resource.SetConnector(defaultConnector);
            var schemas = resource.ListSchemas();

            Assert.Equal(2, schemas.Count());
            Assert.Equal("asch1", schemas.First().Name);
        }

        [FactWithName]
        public void GetResourceRecordsWithOutExpandUserJoins()
        {
            var list = new List<object> { new { a = "a" }, new { a = "b" } }.AsQueryable();
            defaultServices.AddSingleton(c =>
            {
                var mock = new Mock<IDbAdapter>();
                mock.Setup(d => d.GetAll("db1_sch_tbl1")).Returns(list);
                return mock.Object;
            });

            ResetServiceProviderCache();

            var resource = new TestResourceServices(defaultServices.BuildServiceProvider());
            resource.ConfigureAction(defaultConnector, OperationType.read, "tbl1");
            var results = resource.GetResourceRecords(new Dictionary<string, string>());

            schemaInitializer.Verify(c => c.Initilize(), Times.Once);
            Assert.Equal(list, results);
        }

        [FactWithName]
        public void GetResourceRecordsWithExpandUserJoinsPK()
        {
            using (var db = new AppDbContext(appDbOptions))
            {
                db.Connector.Add(new Connector { InitialCatalog = "db1", Schema = "sch", Id = 1 });
                db.UserJoin.Add(new UserJoin
                {
                    Alias = "pkJoin",
                    PKConnectorId = 1,
                    PKTableName = "tbl1",
                    PKColumn = "Id",
                    FKConnectorId = 1,
                    FKTableName = "tbl2",
                    FKColumn = "Id1",
                });

                db.SaveChanges();
            }


            var list = new List<ViewModel1> { new ViewModel1 { Id = 1, Name = "a" }, new ViewModel1 { Id = 2, Name = "b" } }.AsQueryable();
            var ujlist = new List<ViewModel2> { new ViewModel2 { Id1 = 1, Name3 = "a1" }, new ViewModel2 { Id1 = 2, Name3 = "b1" } }.AsQueryable();

            var pkTable = new Table { Database = "db1", Schema = "sch", Name = "tbl1" };
            pkTable.Columns.Add(new Column { PKName = "pk", ColumnName = "Id" });
            pkTable.Columns.Add(new Column { ColumnName = "Name" });

            defaultServices.AddSingleton(c =>
            {
                var mock = new Mock<IDbAdapter>();
                mock.Setup(d => d.GetAll("db1_sch_tbl1")).Returns(list);
                mock.Setup(d => d.GetWithoutContext("db1_sch_tbl2", "db1_sch_tbl1")).Returns(ujlist);
                return mock.Object;
            });
            defaultServices.AddSingleton(c =>
            {
                var mock = new Mock<IDbSchema>();
                mock.Setup(d => d.Tables).Returns(new List<Table>
                {
                    pkTable,
                    new Table{Database = "db1", Schema = "sch", Name = "tbl2"}
                });
                return mock.Object;
            });

            ResetServiceProviderCache();

            var resource = new TestResourceServices(defaultServices.BuildServiceProvider());
            resource.ConfigureAction(defaultConnector, OperationType.read, "tbl1");
            IQueryable<dynamic> results = resource.GetResourceRecords(new Dictionary<string, string>() { { "$expand", "UJ_pkJoinListFromId1" } });

            schemaInitializer.Verify(c => c.Initilize(), Times.Once);
            Assert.Equal("a", results.First().Name);
            Assert.Single(((IEnumerable<dynamic>)results.First().UJ_pkJoinListFromId1));
            Assert.Equal("a1", ((IEnumerable<dynamic>)results.First().UJ_pkJoinListFromId1).First().Name3);
        }

        [FactWithName]
        public void GetResourceRecordsWithExpandUserJoinsPKCompoundKey()
        {
            using (var db = new AppDbContext(appDbOptions))
            {
                db.Connector.Add(new Connector { InitialCatalog = "db1", Schema = "sch", Id = 1 });
                db.UserJoin.Add(new UserJoin
                {
                    Alias = "pkJoin",
                    PKConnectorId = 1,
                    PKTableName = "tbl1",
                    PKColumn = "Id,Name",
                    FKConnectorId = 1,
                    FKTableName = "tbl2",
                    FKColumn = "Id1,Name3",
                });

                db.SaveChanges();
            }


            var list = new List<ViewModel1> { new ViewModel1 { Id = 1, Name = "a" }, new ViewModel1 { Id = 2, Name = "b" } }.AsQueryable();
            var ujlist = new List<ViewModel2> { new ViewModel2 { Id1 = 1, Name3 = "a" }, new ViewModel2 { Id1 = 2, Name3 = "b" } }.AsQueryable();

            var pkTable = new Table { Database = "db1", Schema = "sch", Name = "tbl1" };
            pkTable.Columns.Add(new Column { PKName = "pk", ColumnName = "Id" });
            pkTable.Columns.Add(new Column { ColumnName = "Name" });

            defaultServices.AddSingleton(c =>
            {
                var mock = new Mock<IDbAdapter>();
                mock.Setup(d => d.GetAll("db1_sch_tbl1")).Returns(list);
                mock.Setup(d => d.GetWithoutContext("db1_sch_tbl2", "db1_sch_tbl1")).Returns(ujlist);
                return mock.Object;
            });
            defaultServices.AddSingleton(c =>
            {
                var mock = new Mock<IDbSchema>();
                mock.Setup(d => d.Tables).Returns(new List<Table>
                {
                    pkTable,
                    new Table{Database = "db1", Schema = "sch", Name = "tbl2"}
                });
                return mock.Object;
            });

            ResetServiceProviderCache();

            var resource = new TestResourceServices(defaultServices.BuildServiceProvider());
            resource.ConfigureAction(defaultConnector, OperationType.read, "tbl1");
            IQueryable<dynamic> results = resource.GetResourceRecords(new Dictionary<string, string>() { { "$expand", "UJ_pkJoinListFromId1AndName3" } });

            schemaInitializer.Verify(c => c.Initilize(), Times.Once);
            Assert.Equal("a", results.First().Name);
            Assert.Single(((IEnumerable<dynamic>)results.First().UJ_pkJoinListFromId1AndName3));
            Assert.Equal("a", ((IEnumerable<dynamic>)results.First().UJ_pkJoinListFromId1AndName3).First().Name3);
        }

        [FactWithName]
        public void GetResourceRecordsWithExpandUserJoinsNonValueTypePK()
        {
            using (var db = new AppDbContext(appDbOptions))
            {
                db.Connector.Add(new Connector { InitialCatalog = "db1", Schema = "sch", Id = 1 });
                db.UserJoin.Add(new UserJoin
                {
                    Alias = "pkJoin",
                    PKConnectorId = 1,
                    PKTableName = "tbl1",
                    PKColumn = "Name",
                    FKConnectorId = 1,
                    FKTableName = "tbl2",
                    FKColumn = "Name3",
                });

                db.SaveChanges();
            }


            var list = new List<ViewModel1> { new ViewModel1 { Id = 7, Name = "a" }, new ViewModel1 { Id = 8, Name = "b" } }.AsQueryable();
            var ujlist = new List<ViewModel2> { new ViewModel2 { Id1 = 1, Name3 = "a" }, new ViewModel2 { Id1 = 2, Name3 = "b" } }.AsQueryable();

            var pkTable = new Table { Database = "db1", Schema = "sch", Name = "tbl1" };
            pkTable.Columns.Add(new Column { PKName = "pk", ColumnName = "Id" });
            pkTable.Columns.Add(new Column { ColumnName = "Name" });

            defaultServices.AddSingleton(c =>
            {
                var mock = new Mock<IDbAdapter>();
                mock.Setup(d => d.GetAll("db1_sch_tbl1")).Returns(list);
                mock.Setup(d => d.GetWithoutContext("db1_sch_tbl2", "db1_sch_tbl1")).Returns(ujlist);
                return mock.Object;
            });
            defaultServices.AddSingleton(c =>
            {
                var mock = new Mock<IDbSchema>();
                mock.Setup(d => d.Tables).Returns(new List<Table>
                {
                    pkTable,
                    new Table{Database = "db1", Schema = "sch", Name = "tbl2"}
                });
                return mock.Object;
            });

            ResetServiceProviderCache();

            var resource = new TestResourceServices(defaultServices.BuildServiceProvider());
            resource.ConfigureAction(defaultConnector, OperationType.read, "tbl1");
            IQueryable<dynamic> results = resource.GetResourceRecords(new Dictionary<string, string>() { { "$expand", "UJ_pkJoinListFromName3" } });

            schemaInitializer.Verify(c => c.Initilize(), Times.Once);
            Assert.Equal(7, results.First().Id);
            Assert.Single(((IEnumerable<dynamic>)results.First().UJ_pkJoinListFromName3));
            Assert.Equal(1, ((IEnumerable<dynamic>)results.First().UJ_pkJoinListFromName3).First().Id1);
        }

        [FactWithName]
        public void GetResourceRecordsWithExpandUserJoinsPKShowNullPropertyIfNotInExpand()
        {
            using (var db = new AppDbContext(appDbOptions))
            {
                db.Connector.Add(new Connector { InitialCatalog = "db1", Schema = "sch", Id = 1 });
                db.UserJoin.Add(new UserJoin
                {
                    Alias = "pkJoin",
                    PKConnectorId = 1,
                    PKTableName = "tbl1",
                    PKColumn = "Id",
                    FKConnectorId = 1,
                    FKTableName = "tbl2",
                    FKColumn = "Id1",
                });

                db.SaveChanges();
            }


            var list = new List<ViewModel1> { new ViewModel1 { Id = 1, Name = "a" }, new ViewModel1 { Id = 2, Name = "b" } }.AsQueryable();
            var ujlist = new List<ViewModel2> { new ViewModel2 { Id1 = 1, Name3 = "a1" }, new ViewModel2 { Id1 = 2, Name3 = "b1" } }.AsQueryable();

            var pkTable = new Table { Database = "db1", Schema = "sch", Name = "tbl1" };
            pkTable.Columns.Add(new Column { PKName = "pk", ColumnName = "Id" });
            pkTable.Columns.Add(new Column { ColumnName = "Name" });

            defaultServices.AddSingleton(c =>
            {
                var mock = new Mock<IDbAdapter>();
                mock.Setup(d => d.GetAll("db1_sch_tbl1")).Returns(list);
                mock.Setup(d => d.GetWithoutContext("db1_sch_tbl2", "db1_sch_tbl1")).Returns(ujlist);
                return mock.Object;
            });
            defaultServices.AddSingleton(c =>
            {
                var mock = new Mock<IDbSchema>();
                mock.Setup(d => d.Tables).Returns(new List<Table>
                {
                    pkTable,
                    new Table{Database = "db1", Schema = "sch", Name = "tbl2"}
                });
                return mock.Object;
            });

            ResetServiceProviderCache();

            var resource = new TestResourceServices(defaultServices.BuildServiceProvider());
            resource.ConfigureAction(defaultConnector, OperationType.read, "tbl1");
            IQueryable<dynamic> results = resource.GetResourceRecords(new Dictionary<string, string>());

            schemaInitializer.Verify(c => c.Initilize(), Times.Once);
            Assert.Equal("a", results.First().Name);
             Assert.Null(results.First().UJ_pkJoinListFromId1);
             Assert.NotNull(results.First().GetType().GetProperty("UJ_pkJoinListFromId1"));
        }

        [FactWithName]
        public void GetResourceRecordsWithExpandUserJoinsFK()
        {
            using (var db = new AppDbContext(appDbOptions))
            {
                db.Connector.Add(new Connector { InitialCatalog = "db1", Schema = "sch", Id = 1 });
                db.UserJoin.Add(new UserJoin
                {
                    Alias = "fkJoin",
                    PKConnectorId = 1,
                    PKTableName = "tbl2",
                    PKColumn = "Id1",
                    FKConnectorId = 1,
                    FKTableName = "tbl1",
                    FKColumn = "Id",
                });

                db.SaveChanges();
            }


            var list = new List<ViewModel1> { new ViewModel1 { Id = 1, Name = "a" }, new ViewModel1 { Id = 2, Name = "b" } }.AsQueryable();
            var ujlist = new List<ViewModel2> { new ViewModel2 { Id1 = 1, Name3 = "a1" }, new ViewModel2 { Id1 = 2, Name3 = "b1" } }.AsQueryable();

            var pkTable = new Table { Database = "db1", Schema = "sch", Name = "tbl1" };
            pkTable.Columns.Add(new Column { PKName = "pk", ColumnName = "Id" });
            pkTable.Columns.Add(new Column { ColumnName = "Name" });

            defaultServices.AddSingleton(c =>
            {
                var mock = new Mock<IDbAdapter>();
                mock.Setup(d => d.GetAll("db1_sch_tbl1")).Returns(list);
                mock.Setup(d => d.GetWithoutContext("db1_sch_tbl2", "db1_sch_tbl1")).Returns(ujlist);
                return mock.Object;
            });
            defaultServices.AddSingleton(c =>
            {
                var mock = new Mock<IDbSchema>();
                mock.Setup(d => d.Tables).Returns(new List<Table>
                {
                    pkTable,
                    new Table{Database = "db1", Schema = "sch", Name = "tbl2"}
                });
                return mock.Object;
            });

            ResetServiceProviderCache();

            var resource = new TestResourceServices(defaultServices.BuildServiceProvider());
            resource.ConfigureAction(defaultConnector, OperationType.read, "tbl1");
            IQueryable<dynamic> results = resource.GetResourceRecords(new Dictionary<string, string>() { { "$expand", "UJ_fkJoinListFromId1" } });

            schemaInitializer.Verify(c => c.Initilize(), Times.Once);
            Assert.Equal("a", results.First().Name);
            Assert.Single(((IEnumerable<dynamic>)results.First().UJ_fkJoinListFromId1));
            Assert.Equal("a1", ((IEnumerable<dynamic>)results.First().UJ_fkJoinListFromId1).First().Name3);
        }

        [FactWithName]
        public void GetResourceRecordsWithExpandUserJoinsFKShowNullPropertyIfNotInExpand()
        {
            using (var db = new AppDbContext(appDbOptions))
            {
                db.Connector.Add(new Connector { InitialCatalog = "db1", Schema = "sch", Id = 1 });
                db.UserJoin.Add(new UserJoin
                {
                    Alias = "fkJoin",
                    PKConnectorId = 1,
                    PKTableName = "tbl2",
                    PKColumn = "Id1",
                    FKConnectorId = 1,
                    FKTableName = "tbl1",
                    FKColumn = "Id",
                });

                db.SaveChanges();
            }


            var list = new List<ViewModel1> { new ViewModel1 { Id = 1, Name = "a" }, new ViewModel1 { Id = 2, Name = "b" } }.AsQueryable();
            var ujlist = new List<ViewModel2> { new ViewModel2 { Id1 = 1, Name3 = "a1" }, new ViewModel2 { Id1 = 2, Name3 = "b1" } }.AsQueryable();

            var pkTable = new Table { Database = "db1", Schema = "sch", Name = "tbl1" };
            pkTable.Columns.Add(new Column { PKName = "pk", ColumnName = "Id" });
            pkTable.Columns.Add(new Column { ColumnName = "Name" });

            defaultServices.AddSingleton(c =>
            {
                var mock = new Mock<IDbAdapter>();
                mock.Setup(d => d.GetAll("db1_sch_tbl1")).Returns(list);
                mock.Setup(d => d.GetWithoutContext("db1_sch_tbl2", "db1_sch_tbl1")).Returns(ujlist);
                return mock.Object;
            });
            defaultServices.AddSingleton(c =>
            {
                var mock = new Mock<IDbSchema>();
                mock.Setup(d => d.Tables).Returns(new List<Table>
                {
                    pkTable,
                    new Table{Database = "db1", Schema = "sch", Name = "tbl2"}
                });
                return mock.Object;
            });

            ResetServiceProviderCache();

            var resource = new TestResourceServices(defaultServices.BuildServiceProvider());
            resource.ConfigureAction(defaultConnector, OperationType.read, "tbl1");
            IQueryable<dynamic> results = resource.GetResourceRecords(new Dictionary<string, string>());

            schemaInitializer.Verify(c => c.Initilize(), Times.Once);
            Assert.Equal("a", results.First().Name);
             Assert.Null(results.First().UJ_fkJoinListFromId1);
             Assert.NotNull(results.First().GetType().GetProperty("UJ_fkJoinListFromId1"));
        }

        [FactWithName]
        public void GetResourceById()
        {
            var expected = new object();
            defaultServices.AddSingleton(c =>
            {
                var mock = new Mock<IDbAdapter>();
                mock.Setup(d => d.Find("db1_sch_tbl1", "id1")).Returns(expected);
                return mock.Object;
            });

            ResetServiceProviderCache();

            var resource = new TestResourceServices(defaultServices.BuildServiceProvider());
            resource.ConfigureAction(defaultConnector, OperationType.read, "tbl1");
            var result = resource.GetResourceRecordById("id1");

            schemaInitializer.Verify(c => c.Initilize(), Times.Once);
            Assert.Equal(expected, result);
        }

        [FactWithName]
        public void UpdateTViewModelRecord()
        {
            var model = new ViewModel1 { Id = 1, Name = "newName" };
            var connector = new Connector { Id = 1, Name = "oldName" };
            Connector usedConnector = null;
            var mock = new Mock<IDbAdapter>();

            defaultServices.AddSingleton(c =>
            {
                mock.Setup(d => d.Find("db1_sch_tbl1", "id1")).Returns(connector);
                mock.Setup(d => d.Update(It.IsAny<Connector>()))
                .Callback<object>(d => usedConnector = (Connector)d);

                return mock.Object;
            });

            ResetServiceProviderCache();

            var resource = new TestResourceServices(defaultServices.BuildServiceProvider());
            resource.ConfigureAction(defaultConnector, OperationType.read, "tbl1");
            var result = resource.UpdateResourceRecordById(model, "id1");

            schemaInitializer.Verify(c => c.Initilize(), Times.Once);
            mock.Verify(c => c.Save(), Times.Once);
            Assert.Equal("newName", result.Name);
            Assert.Equal("newName", usedConnector.Name);
            Assert.IsType<ViewModel1>(result);
        }

        [FactWithName]
        public void UpdateTViewModelRecordReturnsNullIfNotFound()
        {
            var model = new ViewModel1 { Id = 1, Name = "newName" };
            var mock = new Mock<IDbAdapter>();

            defaultServices.AddSingleton(c =>
            {
                return mock.Object;
            });

            ResetServiceProviderCache();

            var resource = new TestResourceServices(defaultServices.BuildServiceProvider());
            resource.ConfigureAction(defaultConnector, OperationType.read, "tbl1");
            var result = resource.UpdateResourceRecordById(model, "id12");

            schemaInitializer.Verify(c => c.Initilize(), Times.Once);
            mock.Verify(c => c.Save(), Times.Never);
            Assert.Null(result);
        }

        [FactWithName]
        public void UpdateJTokenRecord()
        {
            var model = JToken.FromObject(new ViewModel1 { Id = 1, Name = "newName" });
            var connector = new Connector { Id = 1, Name = "oldName" };
            Connector usedConnector = null;
            var mock = new Mock<IDbAdapter>();

            defaultServices.AddSingleton(c =>
            {
                mock.Setup(d => d.Find("db1_sch_tbl1", "id1")).Returns(connector);
                mock.Setup(d => d.Update(It.IsAny<Connector>()))
               .Callback<object>(d => usedConnector = (Connector)d);
                return mock.Object;
            });

            ResetServiceProviderCache();

            var resource = new TestResourceServices(defaultServices.BuildServiceProvider());
            resource.ConfigureAction(defaultConnector, OperationType.read, "tbl1");
            dynamic result = resource.UpdateResourceRecordById(model, "id1");

            schemaInitializer.Verify(c => c.Initilize(), Times.Once);
            mock.Verify(c => c.Save(), Times.Once);
            Assert.Equal("newName", result.Name);
            Assert.Equal("newName", usedConnector.Name);
        }

        [FactWithName]
        public void UpdateJTokenRecordReturnsNullIfNotFound()
        {
            var model = JToken.FromObject(new ViewModel1 { Id = 1, Name = "newName" });
            var mock = new Mock<IDbAdapter>();

            defaultServices.AddSingleton(c =>
            {
                return mock.Object;
            });

            ResetServiceProviderCache();

            var resource = new TestResourceServices(defaultServices.BuildServiceProvider());
            resource.ConfigureAction(defaultConnector, OperationType.read, "tbl1");
            var result = resource.UpdateResourceRecordById(model, "id12");

            schemaInitializer.Verify(c => c.Initilize(), Times.Once);
            mock.Verify(c => c.Save(), Times.Never);
             Assert.Null(result);
        }

        [FactWithName]
        public void InsertTViewModelRecord()
        {
            var model = new ViewModel1 { Id = 1, Name = "newName" };
            var connector = new Connector { Id = 1, Name = "oldName" };
            Connector usedConnector = null;
            var mock = new Mock<IDbAdapter>();

            defaultServices.AddSingleton(c =>
            {
                mock.Setup(d => d.NewInstance("db1_sch_tbl1")).Returns(connector);
                mock.Setup(d => d.Add(It.IsAny<Connector>()))
                .Callback<object>(d => usedConnector = (Connector)d);

                return mock.Object;
            });

            ResetServiceProviderCache();

            var resource = new TestResourceServices(defaultServices.BuildServiceProvider());
            resource.ConfigureAction(defaultConnector, OperationType.read, "tbl1");
            var result = resource.CreateNewResourceRecord(model);

            schemaInitializer.Verify(c => c.Initilize(), Times.Once);
            mock.Verify(c => c.Save(), Times.Once);
            Assert.Equal("newName", result.Name);
            Assert.Equal("newName", usedConnector.Name);
            Assert.IsType<ViewModel1>(result);
        }


        [FactWithName]
        public void InsertJTokenRecord()
        {
            var model = JToken.FromObject(new ViewModel1 { Id = 1, Name = "newName" });
            var connector = new Connector { Id = 1, Name = "oldName" };
            Connector usedConnector = null;
            var mock = new Mock<IDbAdapter>();

            defaultServices.AddSingleton(c =>
            {
                mock.Setup(d => d.NewInstance("db1_sch_tbl1")).Returns(connector);
                mock.Setup(d => d.Add(It.IsAny<Connector>()))
                .Callback<object>(d => usedConnector = (Connector)d);

                return mock.Object;
            });

            ResetServiceProviderCache();

            var resource = new TestResourceServices(defaultServices.BuildServiceProvider());
            resource.ConfigureAction(defaultConnector, OperationType.read, "tbl1");
            dynamic result = resource.CreateNewResourceRecord(model);

            schemaInitializer.Verify(c => c.Initilize(), Times.Once);
            mock.Verify(c => c.Save(), Times.Once);
            Assert.Equal("newName", result.Name);
            Assert.Equal("newName", usedConnector.Name);
        }

        [FactWithName]
        public void DeleteRecordReturnsSuccessIfIdFound()
        {
            var connector = new Connector { Id = 1, Name = "oldName" };
            Connector usedConnector = null;
            var mock = new Mock<IDbAdapter>();

            defaultServices.AddSingleton(c =>
            {
                mock.Setup(d => d.Find("db1_sch_tbl1", "id1")).Returns(connector);
                mock.Setup(d => d.Delete(It.IsAny<Connector>()))
                .Callback<object>(d => usedConnector = (Connector)d);

                return mock.Object;
            });


            ResetServiceProviderCache();

            var resource = new TestResourceServices(defaultServices.BuildServiceProvider());
            resource.ConfigureAction(defaultConnector, OperationType.read, "tbl1");
            IDictionary<string, object> result = (IDictionary<string, object>)resource.DeleteResourceRecordById("id1");

            schemaInitializer.Verify(c => c.Initilize(), Times.Once);
            mock.Verify(c => c.Save(), Times.Once);
            Assert.Equal(usedConnector, result["data"]);
            Assert.Equal("Record was successfully deleted.", result["results"]);
        }

        [FactWithName]
        public void DeleteRecordReturnsNullIfNotFound()
        {
            var mock = new Mock<IDbAdapter>();
            defaultServices.AddSingleton(c =>
            {
                return mock.Object;
            });

            ResetServiceProviderCache();

            var resource = new TestResourceServices(defaultServices.BuildServiceProvider());
            resource.ConfigureAction(defaultConnector, OperationType.read, "tbl1");
            IDictionary<string, object> result = (IDictionary<string, object>)resource.DeleteResourceRecordById("id1");

            schemaInitializer.Verify(c => c.Initilize(), Times.Once);
            mock.Verify(c => c.Save(), Times.Never);
             Assert.Null(result["data"]);
            Assert.Equal("No records were deleted.", result["results"]);
        }

        [FactWithName]
        public void GetAllViewRecordsTrimsColumns()
        {
            var list = new List<ViewModel1> { new ViewModel1 { Id = 1, Name = "a2", Name2 = "b" }, new ViewModel1 { Id = 2, Name = "a2", Name2 = "b2" } }.AsQueryable();
            var view = new View
            {
                Database = "db1",
                Schema = "sch",
                Name = "vw1",
            };
            view.Columns.Add(new Column { ColumnName = "Id" });
            view.Columns.Add(new Column { ColumnName = "Name" });

            var mock = new Mock<IDbAdapter>();
            defaultServices.AddSingleton(c =>
            {
                mock.Setup(d => d.GetAll("db1_sch_vw1")).Returns(list);

                return mock.Object;
            });
            defaultServices.AddSingleton(c =>
            {
                var mock2 = new Mock<IDbSchema>();
                mock2.Setup(d => d.Views).Returns(new List<View>
                {
                   view
                });
                return mock2.Object;
            });

            ResetServiceProviderCache();

            var resource = new TestResourceServices(defaultServices.BuildServiceProvider());
            resource.ConfigureAction(defaultConnector, OperationType.read, "vw1");

            var results = resource.GetViewRecords();

            schemaInitializer.Verify(c => c.Initilize(), Times.Once);
             Assert.NotNull(results.ElementType.GetProperty("Id"));
             Assert.NotNull(results.ElementType.GetProperty("Name"));
             Assert.Null(results.ElementType.GetProperty("Name2"));
        }

        [FactWithName]
        public void GetColumnDefinition()
        {
            defaultServices.AddSingleton(c =>
            {
                var mock = new Mock<IDbSchema>();
                mock.SetupGet(d => d.Columns)
                .Returns(new List<Column>
                {
                    new Column
                    {
                        ColumnName = "col1",
                        ColumnType = "type1",
                        ColumnLength = 23,
                        IsComputed = true,
                        ColumnIsNullable = false,
                        PKName = "asdfa",
                        ReferencedDatabase = "refdb",
                        ReferencedTable = "reftbl",
                        ReferencedColumn = "refcol",
                        ReferencedSchema = "refsch",
                        PKIsIdentity = true,
                        Table = new Table{Name = "TBL1"}
                    }
                });
                return mock.Object;
            });

            ResetServiceProviderCache();

            var resource = new TestResourceServices(defaultServices.BuildServiceProvider());
            resource.ConfigureAction(defaultConnector, OperationType.read, "tbl1");

            var result = resource.GetColumnDefinition("COL1");

            schemaInitializer.Verify(c => c.Initilize(), Times.Once);
            Assert.Equal("col1", result.Name);
            Assert.Equal("type1", result.Type);
            Assert.Equal(23, result.Length);
            Assert.True(result.IsComputed);
            Assert.False(result.IsNullable);
            Assert.True(result.IsPrimaryKey);
            Assert.False(result.IsForigenKey);
            Assert.Equal("refdb", result.ReferencedDatabase);
            Assert.Equal("reftbl", result.ReferencedTable);
            Assert.Equal("refcol", result.ReferencedColumn);
            Assert.Equal("refsch", result.ReferencedSchema);
            Assert.True(result.IsPrimaryKeyIdentity);
        }


        [FactWithName]
        public void GetColumnDefinitionColumnNotFound()
        {
            defaultServices.AddSingleton(c =>
            {
                var mock = new Mock<IDbSchema>();
                mock.SetupGet(d => d.Columns)
                .Returns(new List<Column>
                {
                    new Column
                    {
                        ColumnName = "col1",
                        ColumnType = "type1",
                        ColumnLength = 23,
                        IsComputed = true,
                        ColumnIsNullable = false,
                        PKName = "asdfa",
                        ReferencedDatabase = "refdb",
                        ReferencedTable = "reftbl",
                        ReferencedColumn = "refcol",
                        ReferencedSchema = "refsch",
                        PKIsIdentity = true,
                        Table = new Table{Name = "TBL1"}
                    }
                });
                return mock.Object;
            });

            ResetServiceProviderCache();

            var resource = new TestResourceServices(defaultServices.BuildServiceProvider());
            resource.ConfigureAction(defaultConnector, OperationType.read, "tbl1");

            var result = resource.GetColumnDefinition("COLMissing");

            schemaInitializer.Verify(c => c.Initilize(), Times.Once);
             Assert.Null(result);
        }

        [FactWithName]
        public void GetSchmeaDefinition()
        {
            defaultServices.AddSingleton(c =>
            {
                var mock = new Mock<IDbSchema>();
                mock.SetupGet(d => d.Tables)
                .Returns(new List<Table>
                {
                   new Table{Name = "tbl1"}
                });
                mock.SetupGet(d => d.Views)
                  .Returns(new List<View>
                  {
                       new View{Name = "vw1"}
                  });
                mock.SetupGet(d => d.StoredProcedures)
                  .Returns(new List<StoredProcedure>
                  {
                       new StoredProcedure{Name = "sproc1"}
                  });

                return mock.Object;
            });

            ResetServiceProviderCache();

            var resource = new TestResourceServices(defaultServices.BuildServiceProvider());
            resource.ConfigureAction(defaultConnector, OperationType.read, "tbl1");

            var result = resource.GetSchemaDefinition();

            schemaInitializer.Verify(c => c.Initilize(), Times.Once);
            Assert.Equal("tbl1", result.Tables.Single());
            Assert.Equal("vw1", result.Views.Single());
            Assert.Equal("sproc1", result.StoredProcedures.Single());
            Assert.Equal(1, result.ConnectorId);
            Assert.Equal("testConn", result.ConnectorName);
            Assert.Equal("sch", result.Name);
            Assert.Equal("db1", result.DbName);
        }

        [FactWithName]
        public void GetSingleStoredProcedureDefinition()
        {
            var proc = new StoredProcedure
            {
                Name = "sproc1"
            };

            proc.Parameters.Add(new DBParameter
            {
                Name = "param1",
                Type = "type1",
                IsNullable = true,
                IsOutput = false
            });
            defaultServices.AddSingleton(c =>
        {
            var mock = new Mock<IDbSchema>();
            mock.SetupGet(d => d.StoredProcedures)
              .Returns(new List<StoredProcedure> { proc });
            return mock.Object;
        });

            ResetServiceProviderCache();

            var resource = new TestResourceServices(defaultServices.BuildServiceProvider());
            resource.ConfigureAction(defaultConnector, OperationType.read, "SPROC1");

            var result = resource.GetStoredProcedureDefinition();

            schemaInitializer.Verify(c => c.Initilize(), Times.Once);

            Assert.Equal("sproc1", result.Name);
            Assert.Equal("param1", result.Parameters.First().Name);
            Assert.Equal("type1", result.Parameters.First().Type);
             Assert.True(result.Parameters.First().IsNullable);
             Assert.False(result.Parameters.First().IsOutput);
        }
        [FactWithName]
        public void GetSingleStoredProcedurParametereDefinition()
        {
            var proc = new StoredProcedure
            {
                Name = "sproc1"
            };

            proc.Parameters.Add(new DBParameter
            {
                Name = "param1",
                Type = "type1",
                IsNullable = true,
                IsOutput = false
            });
            defaultServices.AddSingleton(c =>
            {
                var mock = new Mock<IDbSchema>();
                mock.SetupGet(d => d.StoredProcedures)
                  .Returns(new List<StoredProcedure> { proc });
                return mock.Object;
            });

            ResetServiceProviderCache();

            var resource = new TestResourceServices(defaultServices.BuildServiceProvider());
            resource.ConfigureAction(defaultConnector, OperationType.read, "SPROC1");

            var result = resource.GetStoredProcedureDefinition("PARAM1");

            schemaInitializer.Verify(c => c.Initilize(), Times.Once);

            Assert.Equal("param1", result.Name);
            Assert.Equal("type1", result.Type);
             Assert.True(result.IsNullable);
             Assert.False(result.IsOutput);
        }

        [FactWithName]
        public void GetTableDefinition()
        {
            var table = new Table { Name = "tbl1", Schema = "sch" };
            table.Columns.Add(new Column
            {
                ColumnName = "col1",
                ColumnType = "type1",
                ColumnLength = 23,
                IsComputed = true,
                ColumnIsNullable = false,
                PKName = "asdfa",
                ReferencedDatabase = "refdb",
                ReferencedTable = "reftbl",
                ReferencedColumn = "refcol",
                ReferencedSchema = "refsch",
                PKIsIdentity = true,
                Table = new Table { Name = "TBL1" }
            });

            defaultServices.AddSingleton(c =>
            {
                var mock = new Mock<IDbSchema>();
                mock.SetupGet(d => d.Tables)
                .Returns(new List<Table> { table });
                return mock.Object;
            });

            ResetServiceProviderCache();

            var resource = new TestResourceServices(defaultServices.BuildServiceProvider());
            resource.ConfigureAction(defaultConnector, OperationType.read, "TBL1");

            var result = resource.GetTableDefinition();
            var column1 = result.Columns.First();

            schemaInitializer.Verify(c => c.Initilize(), Times.Once);

            //table values
            Assert.Equal("tbl1", result.Name);
            Assert.Equal(1, result.ConnectorId);
            Assert.Equal("testConn", result.ConnectorName);
            Assert.Equal("sch", result.SchemaName);
            Assert.Equal("db1", result.DbName);

            //col values
            Assert.Equal("col1", column1.Name);
            Assert.Equal("type1", column1.Type);
            Assert.Equal(23, column1.Length);
            Assert.True(column1.IsComputed);
            Assert.False(column1.IsNullable);
            Assert.True(column1.IsPrimaryKey);
            Assert.False(column1.IsForigenKey);
            Assert.Equal("refdb", column1.ReferencedDatabase);
            Assert.Equal("reftbl", column1.ReferencedTable);
            Assert.Equal("refcol", column1.ReferencedColumn);
            Assert.Equal("refsch", column1.ReferencedSchema);
            Assert.True(column1.IsPrimaryKeyIdentity);
        }

        [FactWithName]
        public void ReplaceInternalServices()
        {
            ClearServiceProviderCache();
            TestResourceServices resource = new TestResourceServices(defaultServices.BuildServiceProvider());

            var serviceReplacer = new Mock<IReplaceServices<OperationResource>>();

            var rootServices = new ServiceCollection();
            rootServices.AddSingleton(serviceReplacer.Object);
            rootServices.AddSingleton(appDbOptions);

            var rootProvider = rootServices.BuildServiceProvider();
            var services = new ServiceCollection();

            resource.ApplyServices(services, rootProvider);

            serviceReplacer.Verify(c => c.ReplaceInternalServices(rootProvider, services), Times.Once);

        }
    }
}