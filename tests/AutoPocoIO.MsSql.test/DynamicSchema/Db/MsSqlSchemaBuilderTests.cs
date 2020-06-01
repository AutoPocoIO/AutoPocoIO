using AutoPocoIO.DynamicSchema.Db;
using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.DynamicSchema.Util;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace AutoPocoIO.test.DynamicSchema.Db
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class MsSqlSchemaBuilderTests
    {
        Config config;
        Mock<MsSqlSchmeaQueries> query;
        Mock<IDbSchema> schema;
        IDbTypeMapper typeMapper;

        [TestInitialize]
        public void Init()
        {
            typeMapper = Mock.Of<IDbTypeMapper>();

            config = new Config { ConnectionString = "Data Source=test" };
            query = new Mock<MsSqlSchmeaQueries>(config);
            schema = new Mock<IDbSchema>();
        }

        [TestMethod]
        public void ResourceTypeIsMsSql()
        {
            var builder = new MsSqlDbSchemaBuilder(query.Object, config, schema.Object, typeMapper);
            Assert.AreEqual(ResourceType.Mssql, builder.ResourceType);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void QueryIsNotNull()
        {
           new MsSqlDbSchemaBuilder(null, config, schema.Object, typeMapper);
        }

        [TestMethod]
        public void ConnectionFromConfig()
        {
            var builder = new MsSqlDbSchemaBuilder(query.Object, config, schema.Object, typeMapper);
            var conn = builder.CreateConnection();

            Assert.AreEqual("Data Source=test", conn.ConnectionString);
            Assert.IsInstanceOfType(conn, typeof(SqlConnection));
        }

        [TestMethod]
        public void ConnectionFromString()
        {
            var builder = new MsSqlDbSchemaBuilder(query.Object, config, schema.Object, typeMapper);
            var conn = builder.CreateConnection("Data Source=test123");

            Assert.AreEqual("Data Source=test123", conn.ConnectionString);
            Assert.IsInstanceOfType(conn, typeof(SqlConnection));
        }

        [TestMethod]
        public void SetupContextOptions()
        {
            var builder = new MsSqlDbSchemaBuilder(query.Object, config, schema.Object, typeMapper);
            var options = builder.CreateDbContextOptions();

            var coreOptions = options.FindExtension<CoreOptionsExtension>();
            var dbOptions = options.FindExtension<SqlServerOptionsExtension>();

            Assert.AreEqual(13, coreOptions.ReplacedServices.Count());
            Assert.AreEqual("Data Source=test", dbOptions.ConnectionString);
        }

        [TestMethod]
        public void CreateCommandForColumns()
        {
            var connection = new SqlConnection();
            query.Setup(c => c.BuildColumns()).Returns("columnCommand");

            var builder = new MsSqlDbSchemaBuilder(query.Object, config, schema.Object, typeMapper);
            var command = VerifyProtectedCommands(builder, connection, "BuildColumnsCommand");

            Assert.AreEqual(CommandType.Text, command.CommandType);
            Assert.AreEqual(connection, command.Connection);
            Assert.AreEqual("columnCommand", command.CommandText);
        }

        [TestMethod]
        public void CreateCommandForStoredProc()
        {
            var connection = new SqlConnection();
            query.Setup(c => c.BuildStoredProcedureCommand()).Returns("spCommand");

            var builder = new MsSqlDbSchemaBuilder(query.Object, config, schema.Object, typeMapper);
            var command = VerifyProtectedCommands(builder, connection, "BuildStoredProcedureCommand");

            Assert.AreEqual(CommandType.Text, command.CommandType);
            Assert.AreEqual(connection, command.Connection);
            Assert.AreEqual("spCommand", command.CommandText);
        }


        [TestMethod]
        public void CreateCommandForTablesAndView()
        {
            var connection = new SqlConnection();
            query.Setup(c => c.BuildTablesViewCommand()).Returns("tableAndView");

            var builder = new MsSqlDbSchemaBuilder(query.Object, config, schema.Object, typeMapper);
            var command = VerifyProtectedCommands(builder, connection, "BuildTablesViewsCommand");

            Assert.AreEqual(CommandType.Text, command.CommandType);
            Assert.AreEqual(connection, command.Connection);
            Assert.AreEqual("tableAndView", command.CommandText);
        }

        private DbCommand VerifyProtectedCommands(MsSqlDbSchemaBuilder builder, SqlConnection conn, string methodName)
        {
            var method = typeof(MsSqlDbSchemaBuilder).GetMethod(methodName,
             BindingFlags.Instance | BindingFlags.NonPublic,
             Type.DefaultBinder,
             new Type[] { typeof(DbConnection) },
             null);

            return (DbCommand)method.Invoke(builder, new[] { conn });
        }
    }
}


