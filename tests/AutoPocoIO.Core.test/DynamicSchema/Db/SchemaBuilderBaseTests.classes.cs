using AutoPocoIO.DynamicSchema.Db;
using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.DynamicSchema.Util;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Data;


namespace AutoPocoIO.test.DynamicSchema.Db
{
    public partial class SchemaBuilderBaseTests
    {
        private class SchemaBuilder1 : DbSchemaBuilderBase
        {
            private IDbConnection createdConn;
            private IDbCommand createdCommand;
            private int cmdCount;

            public DataTable[] Dts { get; set; }
            public DataTable Dt => Dts[cmdCount];
            public SchemaBuilder1(Config config, IDbSchema dbSchema, IDbTypeMapper typeMapper) : base(config, dbSchema, typeMapper)
            {
                Dts = new[] { new DataTable() };
            }

            public override string ResourceType => throw new NotImplementedException();

            public override DataTable ExecuteSchemaCommand(IDbCommand command)
            {
                if (createdCommand == command)
                    return Dts[cmdCount++];

                return null;
            }

            public DataTable ExecuteSchemaCommandBase(IDbCommand command)
            {
                return base.ExecuteSchemaCommand(command);
            }

            public override IDbConnection CreateConnection()
            {
                createdConn = Mock.Of<IDbConnection>();
                return createdConn;

            }

            public override IDbConnection CreateConnection(string connectionString)
            {
                createdConn = Mock.Of<IDbConnection>();
                return createdConn;
            }

            public override DbContextOptions CreateDbContextOptions()
            {
                throw new NotImplementedException();
            }

            protected override IDbCommand BuildColumnsCommand(IDbConnection dbConnection)
            {
                if (dbConnection == createdConn)
                {
                    createdCommand = Mock.Of<IDbCommand>();
                    return createdCommand;
                }
                return null;
            }

            protected override IDbCommand BuildStoredProcedureCommand(IDbConnection dbConnection)
            {
                if (dbConnection == createdConn)
                {
                    createdCommand = Mock.Of<IDbCommand>();
                    return createdCommand;
                }
                return null;
            }

            protected override IDbCommand BuildTablesViewsCommand(IDbConnection dbConnection)
            {
                //if (dbConnection == createdConn)
                //{
                //    createdCommand = Mock.Of<IDbCommand>();
                //    return createdCommand;
                //}
                return null;
            }
        }

        private class SchmeaBuilder2 : DbSchemaBuilderBase
        {
            public SchmeaBuilder2(Config config, IDbSchema dbSchema, IDbTypeMapper typeMapper) : base(config, dbSchema, typeMapper)
            {
            }

            public override string ResourceType => throw new NotImplementedException();

            public override IDbConnection CreateConnection()
            {
                throw new NotImplementedException();
            }

            public override IDbConnection CreateConnection(string connectionString)
            {
                throw new NotImplementedException();
            }

            public override DbContextOptions CreateDbContextOptions()
            {
                throw new NotImplementedException();
            }

            protected override IDbCommand BuildColumnsCommand(IDbConnection dbConnection)
            {
                throw new NotImplementedException();
            }

            protected override IDbCommand BuildStoredProcedureCommand(IDbConnection dbConnection)
            {
                throw new NotImplementedException();
            }

            protected override IDbCommand BuildTablesViewsCommand(IDbConnection dbConnection)
            {
                throw new NotImplementedException();
            }
        }
    }
}
