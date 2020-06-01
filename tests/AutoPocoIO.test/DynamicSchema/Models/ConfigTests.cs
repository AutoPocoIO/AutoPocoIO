using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.Models;
using Xunit;
using System.Collections.Generic;

namespace AutoPocoIO.test.DynamicSchema.Models
{
    
     [Trait("Category", TestCategories.Unit)]
    public class ConfigTests
    {
        [FactWithName]
        public void NoJoinsReturnsEmptyString()
        {
            var config = new Config();
            Assert.Equal("''", config.JoinsAsString);
        }

        [FactWithName]
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

            Assert.Equal("Object_ID('sch1.pktbl'),Object_ID('sch1.fktbl')", config.JoinsAsString);
        }

        [FactWithName]
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

            Assert.Equal("Object_ID('sch1.fktbl')", config.JoinsAsString);
        }

        [FactWithName]
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

            Assert.Equal("''", config.JoinsAsString);
        }
    }
}
