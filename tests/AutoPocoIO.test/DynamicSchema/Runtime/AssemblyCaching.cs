using AutoPocoIO.DynamicSchema.Db;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.DynamicSchema.Runtime;
using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace AutoPocoIO.test.DynamicSchema.Runtime
{
    [Trait("Category", TestCategories.Unit)]
    [Serializable]
    public class AssemblyCaching
    {
        private readonly string tableSuffix;
        private int GetPrivateTypeBuilderCount(DynamicClassBuilder classBuilder)
        {
            FieldInfo strProperty = classBuilder.GetType().GetField("_typeBuilders", BindingFlags.NonPublic | BindingFlags.Instance);
            return ((Dictionary<string, TypeBuilder>)strProperty.GetValue(classBuilder)).Count();
        }

        public AssemblyCaching()
        {
            tableSuffix = "_" + Guid.NewGuid().ToString();
        }

        [FactWithName]
        public void AddForigenKeyUpdatesForcesRegen()
        {
            Table tbl1 = new Table
            {
                Name = "tbl1" + tableSuffix,
                Schema = "sch1",
                Database = "db1",
            };

            tbl1.Columns.AddRange(
                 new List<Column>
                {
                    new Column
                    {
                        TableSchema = "sch1",
                        TableName ="tbl1"+ tableSuffix,
                        ColumnName = "Id1",
                        ColumnType = "int",
                        PKName = "pk",
                         DataType = new DataType
                        {
                            SystemType = typeof(int)
                        }
                    },
                    new Column
                    {

                        TableSchema = "sch1",
                        TableName ="tbl1"+ tableSuffix,
                        ColumnName = "fkCol",
                        ColumnType = "int",
                         DataType = new DataType
                        {
                            SystemType = typeof(int)
                        }
                    }
                });

            Table tbl2 = new Table
            {
                Name = "tbl2" + tableSuffix,
                Schema = "sch1",
                Database = "db1"
            };

            tbl2.Columns.AddRange(
                new List<Column>
                {
                    new Column
                    {
                        TableSchema = "sch1",
                        TableName ="tbl2"+ tableSuffix,
                        ColumnName = "Id2",
                        ColumnType = "int",
                        PKName = "pk",
                         DataType = new DataType
                        {
                            SystemType = typeof(int)
                        }
                    },
                    new Column
                    {

                        TableSchema = "sch1",
                        TableName ="tbl2"+ tableSuffix,
                        ColumnName = "name",
                        ColumnType = "varchar",
                         DataType = new DataType
                        {
                            SystemType = typeof(string)
                        }
                    }
                });

            var initList = new List<Table> { tbl1, tbl2 };

            var dbSchema = new Mock<IDbSchema>();
            dbSchema.SetupGet(c => c.Tables).Returns(initList);
            dbSchema.SetupGet(c => c.Views).Returns(new List<View>());

            DynamicClassBuilder classBuilder = new DynamicClassBuilder(dbSchema.Object);
            classBuilder.CreateModelTypes("tbl1" + tableSuffix);

            //Created types and total types are the same
            Assert.Equal(2, GetPrivateTypeBuilderCount(classBuilder));
            Assert.Equal(2, classBuilder.ExistingAssemblies.Count);

            //Properties are just the columns
            Assert.Equal(2, classBuilder.ExistingAssemblies.Values.First().GetProperties().Length);
            Assert.Equal(2, classBuilder.ExistingAssemblies.Values.Last().GetProperties().Length);

            //Keep same columns but link to table 1
            tbl2.Columns.Clear();
            tbl2.Columns.AddRange(new List<Column>
            {
                 new Column
                    {
                        TableSchema = "sch1",
                        TableName ="tbl2"+ tableSuffix,
                        ColumnName = "Id2",
                        ColumnType = "int",
                        PKName = "pk",
                        DataType = new DataType
                        {
                            SystemType = typeof(int)
                        }
                    },
                    new Column
                    {

                        TableSchema = "sch1",
                        TableName ="tbl2"+ tableSuffix,
                        ColumnName = "name",
                        ColumnType = "varchar",
                        FKName = "fk",
                        ReferencedDatabase = "db1",
                        ReferencedSchema = "sch1",
                        ReferencedTable = "tbl1"+ tableSuffix,
                        ReferencedColumn = "Id2",
                         DataType = new DataType
                        {
                            SystemType = typeof(string)
                        }
                    }
            });



            var dbSchema2 = new Mock<IDbSchema>();
            dbSchema2.SetupGet(c => c.Tables).Returns(initList);
            dbSchema2.SetupGet(c => c.Views).Returns(new List<View>());

            DynamicClassBuilder classBuilder2 = new DynamicClassBuilder(dbSchema2.Object);
            classBuilder2.CreateModelTypes("tbl1" + tableSuffix);

            //Created types and total types are the same
            Assert.Equal(2, GetPrivateTypeBuilderCount(classBuilder2));
            Assert.Equal(2, classBuilder2.ExistingAssemblies.Count);

            //Properties are include the columns and navproperties
            Assert.Equal(3, classBuilder2.ExistingAssemblies.Values.First().GetProperties().Length);
            Assert.Equal(3, classBuilder2.ExistingAssemblies.Values.Last().GetProperties().Length);

        }


        [FactWithName]
        public void UpdateFkTableForceRegenAll()
        {

            Table tbl1 = new Table
            {
                Name = "tbl1" + tableSuffix,
                Schema = "sch1",
                Database = "db1",
            };
            tbl1.Columns.AddRange(
                new List<Column>
                {
                    new Column
                    {
                        TableSchema = "sch1",
                        TableName ="tbl1"+ tableSuffix,
                        ColumnName = "Id1",
                        ColumnType = "int",
                        PKName = "pk",
                         DataType = new DataType
                        {
                            SystemType = typeof(int)
                        }
                    },
                    new Column
                    {

                        TableSchema = "sch1",
                        TableName ="tbl1"+ tableSuffix,
                        ColumnName = "fkCol",
                        ColumnType = "int",
                         DataType = new DataType
                        {
                            SystemType = typeof(int)
                        }
                    }
                });

            Table tbl2 = new Table
            {
                Name = "tbl2" + tableSuffix,
                Schema = "sch1",
                Database = "db1",
            };

            tbl2.Columns.AddRange(
                new List<Column>
                {
                    new Column
                    {
                        TableSchema = "sch1",
                        TableName ="tbl2"+ tableSuffix,
                        ColumnName = "Id2",
                        ColumnType = "int",
                        PKName = "pk",
                         DataType = new DataType
                        {
                            SystemType = typeof(int)
                        }
                    },
                    new Column
                    {

                        TableSchema = "sch1",
                        TableName ="tbl2"+ tableSuffix,
                        ColumnName = "name",
                        ColumnType = "varchar",
                        FKName = "fk",
                        ReferencedDatabase = "db1",
                        ReferencedSchema = "sch1",
                        ReferencedTable = "tbl1"+ tableSuffix,
                        ReferencedColumn = "Id2",
                         DataType = new DataType
                        {
                            SystemType = typeof(string)
                        }
                    }
                });

            var initList = new List<Table> { tbl1, tbl2 };

            var dbSchema = new Mock<IDbSchema>();
            dbSchema.SetupGet(c => c.Tables).Returns(initList);
            dbSchema.SetupGet(c => c.Views).Returns(new List<View>());

            DynamicClassBuilder classBuilder = new DynamicClassBuilder(dbSchema.Object);

            classBuilder.CreateModelTypes("tbl1" + tableSuffix);

            //Created types and total types are the same
            Assert.Equal(2, GetPrivateTypeBuilderCount(classBuilder));
            Assert.Equal(2, classBuilder.ExistingAssemblies.Count);

            //Properties are just the columns
            Assert.Equal(3, classBuilder.ExistingAssemblies.Values.First().GetProperties().Length);
            Assert.Equal(3, classBuilder.ExistingAssemblies.Values.Last().GetProperties().Length);


            //Add column to tbl2
            tbl2.Columns.Clear();
            tbl2.Columns.AddRange(new List<Column>
            {
                 new Column
                 {
                    TableSchema = "sch1",
                    TableName ="tbl2"+ tableSuffix,
                    ColumnName = "Id2",
                    ColumnType = "int",
                    PKName = "pk",
                     DataType = new DataType
                        {
                            SystemType = typeof(int)
                        }
                 },
                 new Column
                 {

                    TableSchema = "sch1",
                    TableName ="tbl2"+ tableSuffix,
                    ColumnName = "name",
                    ColumnType = "varchar",
                    FKName = "fk",
                    ReferencedDatabase = "db1",
                    ReferencedSchema = "sch1",
                    ReferencedTable = "tbl1"+ tableSuffix,
                    ReferencedColumn = "Id2",
                     DataType = new DataType
                        {
                            SystemType = typeof(string)
                        }
                 },
                 new Column
                 {
                    TableSchema = "sch1",
                    TableName ="tbl2"+ tableSuffix,
                    ColumnName = "newCol",
                    ColumnType = "int",
                     DataType = new DataType
                        {
                            SystemType = typeof(int)
                        }
                 },
            });


            var dbSchema2 = new Mock<IDbSchema>();
            dbSchema2.SetupGet(c => c.Tables).Returns(initList);
            dbSchema2.SetupGet(c => c.Views).Returns(new List<View>());

            DynamicClassBuilder classBuilder2 = new DynamicClassBuilder(dbSchema2.Object);

            classBuilder2.CreateModelTypes("tbl1" + tableSuffix);

            //Created types and total types are the same
            Assert.Equal(2, GetPrivateTypeBuilderCount(classBuilder2));
            Assert.Equal(2, classBuilder2.ExistingAssemblies.Count);

            //Properties are include the columns and navproperties
            Assert.Equal(3, classBuilder2.ExistingAssemblies.Values.First().GetProperties().Length);
            Assert.Equal(4, classBuilder2.ExistingAssemblies.Values.Last().GetProperties().Length);
        }
    }
}
