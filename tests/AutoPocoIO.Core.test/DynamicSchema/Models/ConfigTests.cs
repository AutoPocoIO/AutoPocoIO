using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace AutoPocoIO.test.DynamicSchema.Models
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class ConfigTests
    {
        [TestMethod]
        public void NoJoinsReturnsEmptyString()
        {
            var config = new Config();
            Assert.AreEqual("''", config.JoinsAsString);
        }

        [TestMethod]
        public void UnionPKAndFkInformation()
        {
            var config = new Config
            {
                UserDefinedJoins = new List<UserJoinConfiguration>
                {
                    new UserJoinConfiguration
                    {
                        PrincipalSchema = "sch1",
                        PrincipalTable = "pktbl",
                        DependentSchema = "sch1",
                        DependentTable = "fktbl"
                    }
                }
            };

            Assert.AreEqual("Object_ID('sch1.pktbl'),Object_ID('sch1.fktbl')", config.JoinsAsString);
        }

        [TestMethod]
        public void ExcludeFilterSelectedTableFromJoins()
        {
            var config = new Config()
            {
                FilterSchema = "sch1",
                IncludedTable = "pktbl"
            };

            config.UserDefinedJoins = new List<UserJoinConfiguration>
            {
                new UserJoinConfiguration
                {
                    PrincipalSchema = "sch1",
                    PrincipalTable = "pktbl",
                    DependentSchema = "sch1",
                    DependentTable = "fktbl"
                }
            };

            Assert.AreEqual("Object_ID('sch1.fktbl')", config.JoinsAsString);
        }

        [TestMethod]
        public void ReturnEmtpyCharIfPkAndFkReferenceSelectedTable()
        {
            var config = new Config()
            {
                FilterSchema = "sch1",
                IncludedTable = "pktbl"
            };

            config.UserDefinedJoins = new List<UserJoinConfiguration>
            {
                new UserJoinConfiguration
                {
                    PrincipalSchema = "sch1",
                    PrincipalTable = "pktbl",
                    DependentSchema = "sch1",
                    DependentTable = "pktbl"
                }
            };

            Assert.AreEqual("''", config.JoinsAsString);
        }
    }
}
