using AutoPocoIO.DynamicSchema.Db;
using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.DynamicSchema.Util;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Xunit;
using Moq;
using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace AutoPocoIO.MsSql.test.DynamicSchema.Db
{
    
    [Trait("Category", TestCategories.Unit)]
    public class MsSqlSchemaBuilderTests
    {
        Config config;
        Mock<MsSqlSchmeaQueries> query;
        Mock<IDbSchema> schema;
        IDbTypeMapper typeMapper;

        public MsSqlSchemaBuilderTests()
        {
            typeMapper = Mock.Of<IDbTypeMapper>();

            config = new Config { ConnectionString = "Data Source=test" };
            query = new Mock<MsSqlSchmeaQueries>(config);
            schema = new Mock<IDbSchema>();
        }

        [FactWithName]
        public void ResourceTypeIsMsSql()
        {
            var builder = new MsSqlDbSchemaBuilder(query.Object, config, schema.Object, typeMapper);
            Assert.Equal(ResourceType.Mssql, builder.ResourceType);
        }

        [FactWithName]
        public void QueryIsNotNull()
        {
            void act() => new MsSqlDbSchemaBuilder(null, config, schema.Object, typeMapper);
            Assert.Throws<ArgumentNullException>(act);
        }

        [FactWithName]
        public void ConnectionFromConfig()
        {
            var builder = new MsSqlDbSchemaBuilder(query.Object, config, schema.Object, typeMapper);
            var conn = builder.CreateConnection();

            Assert.Equal("Data Source=test", conn.ConnectionString);
            Assert.IsType<SqlConnection>(conn);
        }

        [FactWithName]
        public void ConnectionFromString()
        {
            var builder = new MsSqlDbSchemaBuilder(query.Object, config, schema.Object, typeMapper);
            var conn = builder.CreateConnection("Data Source=test123");

            Assert.Equal("Data Source=test123", conn.ConnectionString);
            Assert.IsType<SqlConnection>(conn);
        }

        [FactWithName]
        public void SetupContextOptions()
        {
            var builder = new MsSqlDbSchemaBuilder(query.Object, config, schema.Object, typeMapper);
            var options = builder.CreateDbContextOptions();

            var coreOptions = options.FindExtension<CoreOptionsExtension>();
            var dbOptions = options.FindExtension<SqlServerOptionsExtension>();

            Assert.Equal(13, coreOptions.ReplacedServices.Count());
            Assert.Equal("Data Source=test", dbOptions.ConnectionString);
        }

        [FactWithName]
        public void CreateCommandForColumns()
        {
            var connection = new SqlConnection();
            query.Setup(c => c.BuildColumns()).Returns("columnCommand");

            var builder = new MsSqlDbSchemaBuilder(query.Object, config, schema.Object, typeMapper);
            var command = VerifyProtectedCommands(builder, connection, "BuildColumnsCommand");

            Assert.Equal(CommandType.Text, command.CommandType);
            Assert.Equal(connection, command.Connection);
            Assert.Equal("columnCommand", command.CommandText);
        }

        [FactWithName]
        public void CreateCommandForStoredProc()
        {
            var connection = new SqlConnection();
            query.Setup(c => c.BuildStoredProcedureCommand()).Returns("spCommand");

            var builder = new MsSqlDbSchemaBuilder(query.Object, config, schema.Object, typeMapper);
            var command = VerifyProtectedCommands(builder, connection, "BuildStoredProcedureCommand");

            Assert.Equal(CommandType.Text, command.CommandType);
            Assert.Equal(connection, command.Connection);
            Assert.Equal("spCommand", command.CommandText);
        }


        [FactWithName]
        public void CreateCommandForTablesAndView()
        {
            var connection = new SqlConnection();
            query.Setup(c => c.BuildTablesViewCommand()).Returns("tableAndView");

            var builder = new MsSqlDbSchemaBuilder(query.Object, config, schema.Object, typeMapper);
            var command = VerifyProtectedCommands(builder, connection, "BuildTablesViewsCommand");

            Assert.Equal(CommandType.Text, command.CommandType);
            Assert.Equal(connection, command.Connection);
            Assert.Equal("tableAndView", command.CommandText);
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


