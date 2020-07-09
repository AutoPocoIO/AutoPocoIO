using AutoPocoIO.DynamicSchema.Db;
using AutoPocoIO.MsSql.DynamicSchema.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace AutoPocoIO.test.Extensions
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class StoredProcedureTests
    {
        private Mock<IDbCommand> Command;
        private Mock<IDbConnection> Connection;
        private Mock<IDataReader> DataReader;
        private IDbAdapter DbAdapter;

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

            var dbMock = new Mock<IDbAdapter>();
            dbMock.Setup(c => c.CreateDbCommand())
                .Returns(Command.Object);

            DbAdapter = dbMock.Object;
        }


        [TestMethod]
        public void GetListOpenConnection()
        {
            Init(ConnectionState.Closed);

            DbAdapter.DynamicListFromSql("sql", null);
            Connection.Verify(c => c.Open(), Times.Once);
        }

        [TestMethod]
        public void GetListSkipOpenConnectionIfOpen()
        {
            Init(ConnectionState.Open);
            DbAdapter.DynamicListFromSql("sql", null);
            Connection.Verify(c => c.Open(), Times.Never);
        }


        [TestMethod]
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


            var results = DbAdapter.DynamicListFromSql("sql", null);
            Assert.AreEqual(1, results.Count);

            var resultSet = (List<IDictionary<string, object>>)results["ResultSet"];
            Assert.AreEqual(2, resultSet.Count);
            Assert.AreEqual(1, resultSet[0]["Name1"]);
            Assert.AreEqual("Test1", resultSet[0]["Name2"]);
            Assert.AreEqual(11, resultSet[1]["Name1"]);
            Assert.AreEqual("SecondVal", resultSet[1]["Name2"]);

        }

        [TestMethod]
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

            var results = DbAdapter.DynamicListFromSql("sql", null);
            Assert.AreEqual(2, results.Count);

            var resultSet = (List<IDictionary<string, object>>)results["ResultSet"];
            Assert.AreEqual(2, resultSet.Count);
            Assert.AreEqual(1, resultSet[0]["Name1"]);
            Assert.AreEqual(11, resultSet[1]["Name1"]);

            var resultSet2 = (List<IDictionary<string, object>>)results["ResultSet1"];
            Assert.AreEqual(1, resultSet2.Count);
            Assert.AreEqual("second set", resultSet2[0]["Name1"]);
        }

        [TestMethod]
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


            var results = DbAdapter.DynamicListFromSql("sql", null);
            Assert.AreEqual(1, results.Count);

            var resultSet = (List<IDictionary<string, object>>)results["ResultSet"];
            Assert.AreEqual(1, resultSet.Count);
            Assert.AreEqual(1, resultSet[0]["Column0"]);
            Assert.AreEqual("Test1", resultSet[0]["Column1"]);
        }

        [TestMethod]
        public void ListFromSqlWithOutputParams()
        {
            Init(ConnectionState.Open);

            var outputParam = new Mock<DbParameter>();
            outputParam.SetupGet(c => c.ParameterName).Returns("param1");
            outputParam.SetupGet(c => c.Direction).Returns(ParameterDirection.Output);
            outputParam.SetupGet(c => c.Value).Returns(123);

            var results = DbAdapter.DynamicListFromSql("sql", outputParam.Object);
            Assert.AreEqual(2, results.Count);

            var resultSet = (List<IDictionary<string, object>>)results["ResultSet"];
            Assert.AreEqual(0, resultSet.Count);
            Assert.AreEqual(123, results["param1"]);
        }
    }
}
