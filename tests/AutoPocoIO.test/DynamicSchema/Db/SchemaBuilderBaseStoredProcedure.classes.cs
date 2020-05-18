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
    public partial class SchmeaBuilderBaseStoredProcedure
    {
        private class SchemaBuilder1 : DbSchemaBuilderBase
        {
            private IDbConnection createdConn;
            private IDbCommand createdCommand;

            public DataTable dtProcs { get; set; }

            public SchemaBuilder1(Config config, IDbSchema dbSchema, IDbTypeMapper typeMapper) : base(config, dbSchema, typeMapper)
            {
                dtProcs = new DataTable();
            }

            public override ResourceType ResourceType => throw new NotImplementedException();

            public override DataTable ExecuteSchemaCommand(IDbCommand command)
            {
                if (createdCommand == command)
                {
                        return dtProcs;
                }

                return null;
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
                throw new NotImplementedException();
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
                if (dbConnection == createdConn)
                {
                    createdCommand = Mock.Of<IDbCommand>();
                    return createdCommand;
                }
                return null;
            }
        }

    }
}
