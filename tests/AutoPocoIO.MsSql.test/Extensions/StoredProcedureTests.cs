using AutoPocoIO.DynamicSchema.Db;
using AutoPocoIO.MsSql.DynamicSchema.Runtime;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace AutoPocoIO.MsSql.test.Extensions
{
    
    [Trait("Category", TestCategories.Unit)]
    public class StoredProcedureTests
    {
        private readonly DbContextOptions dbOptions = new DbContextOptionsBuilder()
             .UseInMemoryDatabase("logDb" + Guid.NewGuid().ToString())
             .Options;

        private Mock<IDbCommand> Command;
        private Mock<IDbConnection> Connection;
        private Mock<IDataReader> DataReader;
        private DbContextBase Context;

        public void Init(ConnectionState connectionState)
        {
            var paramCollection = new Mock<DbParameterCollection>().Object;
            Connection = new Mock<IDbConnection>();
            Connection.Setup(c => c.Open()).Verifiable();
            Connection.Setup(c => c.State)
                .Returns(connectionState);

            Command = new Mock<IDbCommand>();
            Command.SetupGet(c => c.Connection)
                .Returns(Connection.Object);
            Command.SetupGet(c => c.Parameters)
                .Returns(paramCollection);


            DataReader = new Mock<IDataReader>();
            Command.Setup(c => c.ExecuteReader())
                .Returns(DataReader.Object);

            var dbMock = new Mock<DbContextBase>(dbOptions, null, null);
            dbMock.Setup(c => c.CreateDbCommand())
                .Returns(Command.Object);

            Context = dbMock.Object;
        }


        [FactWithName]
        public void GetListOpenConnection()
        {
            Init(ConnectionState.Closed);

            Context.DynamicListFromSql("sql", null);
            Connection.Verify(c => c.Open(), Times.Once);
        }

        [FactWithName]
        public void GetListSkipOpenConnectionIfOpen()
        {
            Init(ConnectionState.Open);
            Context.DynamicListFromSql("sql", null);
            Connection.Verify(c => c.Open(), Times.Never);
        }


        [FactWithName]
        public void GetListFromSql()
        {

            Init(ConnectionState.Open);

            DataReader.Setup(c => c.FieldCount).Returns(2);
            DataReader.Setup(c => c.GetName(0)).Returns("Name1");
            DataReader.Setup(c => c.GetName(1)).Returns("Name2");

            DataReader.SetupSequence(c => c[0])
               .Returns(1)
               .Returns(11);

            DataReader.SetupSequence(c => c[1])
              .Returns("Test1")
              .Returns("SecondVal");


            DataReader.SetupSequence(m => m.Read())
                        .Returns(true) // Read the first row
                        .Returns(true) // Read the second row
                        .Returns(false); // Done reading


            var results = Context.DynamicListFromSql("sql", null);
            Assert.Equal(1, results.Count);

            var resultSet = (List<IDictionary<string, object>>)results["ResultSet"];
            Assert.Equal(2, resultSet.Count);
            Assert.Equal(1, resultSet[0]["Name1"]);
            Assert.Equal("Test1", resultSet[0]["Name2"]);
            Assert.Equal(11, resultSet[1]["Name1"]);
            Assert.Equal("SecondVal", resultSet[1]["Name2"]);

        }

        [FactWithName]
        public void GetMultipleResultListFromSql()
        {

            Init(ConnectionState.Open);

            DataReader.Setup(c => c.FieldCount).Returns(1);
            DataReader.Setup(c => c.GetName(0)).Returns("Name1");

            DataReader.SetupSequence(c => c[0])
               .Returns(1)
               .Returns(11)
               .Returns("second set");


            DataReader.SetupSequence(m => m.Read())
                        .Returns(true) // Read the first row
                        .Returns(true) // Read the second row
                        .Returns(false) // Done reading set 1
                        .Returns(true) // Read the first row set 2
                        .Returns(false); // Done reading set 2

            DataReader.SetupSequence(m => m.NextResult())
                       .Returns(true) // Read the second set
                       .Returns(false);

            var results = Context.DynamicListFromSql("sql", null);
            Assert.Equal(2, results.Count);

            var resultSet = (List<IDictionary<string, object>>)results["ResultSet"];
            Assert.Equal(2, resultSet.Count);
            Assert.Equal(1, resultSet[0]["Name1"]);
            Assert.Equal(11, resultSet[1]["Name1"]);

            var resultSet2 = (List<IDictionary<string, object>>)results["ResultSet1"];
            Assert.Equal(1, resultSet2.Count);
            Assert.Equal("second set", resultSet2[0]["Name1"]);
        }

        [FactWithName]
        public void ListFromSqlNoColumnNames()
        {
            Init(ConnectionState.Open);

            DataReader.Setup(c => c.FieldCount).Returns(2);
            DataReader.Setup(c => c.GetName(0)).Returns("");

            DataReader.Setup(c => c[0]).Returns(1);
            DataReader.Setup(c => c[1]).Returns("Test1");

            DataReader.SetupSequence(m => m.Read())
                        .Returns(true) // Read the first row
                        .Returns(false); // Done reading


            var results = Context.DynamicListFromSql("sql", null);
            Assert.Equal(1, results.Count);

            var resultSet = (List<IDictionary<string, object>>)results["ResultSet"];
            Assert.Equal(1, resultSet.Count);
            Assert.Equal(1, resultSet[0]["Column0"]);
            Assert.Equal("Test1", resultSet[0]["Column1"]);
        }

        [FactWithName]
        public void ListFromSqlWithOutputParams()
        {
            Init(ConnectionState.Open);

            var outputParam = new Mock<DbParameter>();
            outputParam.SetupGet(c => c.ParameterName).Returns("param1");
            outputParam.SetupGet(c => c.Direction).Returns(ParameterDirection.Output);
            outputParam.SetupGet(c => c.Value).Returns(123);

            var results = Context.DynamicListFromSql("sql", outputParam.Object);
            Assert.Equal(2, results.Count);

            var resultSet = (List<IDictionary<string, object>>)results["ResultSet"];
            Assert.Equal(0, resultSet.Count);
            Assert.Equal(123, results["param1"]);
        }
    }
}
