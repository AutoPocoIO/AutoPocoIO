using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.DynamicSchema.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AutoPocoIO.test.DynamicSchema.Db
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class DbTypeMapperTests
    {
        private IDbTypeMapper mapper;

        [TestInitialize]
        public void Init()
        {
            mapper = new DbTypeMapper();
        }

        [TestMethod]
        public void UniqueIdentifierToGuid()
        {
            var column = new Column { ColumnType = "uniqueIdentifier" };
            var result = mapper.DBTypeToDataType(column);

            Assert.AreEqual("uniqueIdentifier", result.DbType);
            Assert.AreEqual(typeof(Guid?), result.SystemType);
        }

        [TestMethod]
        public void XMLToString()
        {
            var column = new Column { ColumnType = "xml" };
            var result = mapper.DBTypeToDataType(column);

            Assert.AreEqual("xml", result.DbType);
            Assert.AreEqual(typeof(string), result.SystemType);
        }

        [TestMethod]
        public void NvarcharToString()
        {
            var column = new Column { ColumnType = "nvarchar" };
            var result = mapper.DBTypeToDataType(column);

            Assert.AreEqual("nvarchar", result.DbType);
            Assert.AreEqual(typeof(string), result.SystemType);
        }

        [TestMethod]
        public void CharToString()
        {
            var column = new Column { ColumnType = "char" };
            var result = mapper.DBTypeToDataType(column);

            Assert.AreEqual("char", result.DbType);
            Assert.AreEqual(typeof(string), result.SystemType);
        }

        [TestMethod]
        public void NCharToString()
        {
            var column = new Column { ColumnType = "nchar" };
            var result = mapper.DBTypeToDataType(column);

            Assert.AreEqual("nchar", result.DbType);
            Assert.AreEqual(typeof(string), result.SystemType);
        }

        [TestMethod]
        public void TextToString()
        {
            var column = new Column { ColumnType = "text" };
            var result = mapper.DBTypeToDataType(column);

            Assert.AreEqual("text", result.DbType);
            Assert.AreEqual(typeof(string), result.SystemType);
        }

        [TestMethod]
        public void VarCharToString()
        {
            var column = new Column { ColumnType = "VarChar" };
            var result = mapper.DBTypeToDataType(column);

            Assert.AreEqual("VarChar", result.DbType);
            Assert.AreEqual(typeof(string), result.SystemType);
        }

        [TestMethod]
        public void VarChar2ToString()
        {
            var column = new Column { ColumnType = "VarChar2" };
            var result = mapper.DBTypeToDataType(column);

            Assert.AreEqual("VarChar2", result.DbType);
            Assert.AreEqual(typeof(string), result.SystemType);
        }

        [TestMethod]
        public void SysNameToString()
        {
            var column = new Column { ColumnType = "sysName" };
            var result = mapper.DBTypeToDataType(column);

            Assert.AreEqual("sysName", result.DbType);
            Assert.AreEqual(typeof(string), result.SystemType);
        }

        [TestMethod]
        public void MoneyToDecimal()
        {
            var column = new Column { ColumnType = "Money" };
            var result = mapper.DBTypeToDataType(column);

            Assert.AreEqual("Money", result.DbType);
            Assert.AreEqual(typeof(decimal?), result.SystemType);
        }

        [TestMethod]
        public void SmallMoneyDecimal()
        {
            var column = new Column { ColumnType = "SmallMoney" };
            var result = mapper.DBTypeToDataType(column);

            Assert.AreEqual("SmallMoney", result.DbType);
            Assert.AreEqual(typeof(decimal?), result.SystemType);
        }

        [TestMethod]
        public void NumericToString()
        {
            var column = new Column { ColumnType = "Numeric" };
            var result = mapper.DBTypeToDataType(column);

            Assert.AreEqual("Numeric", result.DbType);
            Assert.AreEqual(typeof(decimal?), result.SystemType);
        }

        [TestMethod]
        public void DecimalToDecimal()
        {
            var column = new Column { ColumnType = "Decimal" };
            var result = mapper.DBTypeToDataType(column);

            Assert.AreEqual("Decimal", result.DbType);
            Assert.AreEqual(typeof(decimal?), result.SystemType);
        }

        [TestMethod]
        public void NumberToDecimal()
        {
            var column = new Column { ColumnType = "Number" };
            var result = mapper.DBTypeToDataType(column);

            Assert.AreEqual("Number", result.DbType);
            Assert.AreEqual(typeof(decimal?), result.SystemType);
        }

        [TestMethod]
        public void TinyIntToByte()
        {
            var column = new Column { ColumnType = "TinyInt" };
            var result = mapper.DBTypeToDataType(column);

            Assert.AreEqual("TinyInt", result.DbType);
            Assert.AreEqual(typeof(byte?), result.SystemType);
        }

        [TestMethod]
        public void SmallIntToShort()
        {
            var column = new Column { ColumnType = "SmallInt" };
            var result = mapper.DBTypeToDataType(column);

            Assert.AreEqual("SmallInt", result.DbType);
            Assert.AreEqual(typeof(short?), result.SystemType);
        }

        [TestMethod]
        public void IntToInt()
        {
            var column = new Column { ColumnType = "Int" };
            var result = mapper.DBTypeToDataType(column);

            Assert.AreEqual("Int", result.DbType);
            Assert.AreEqual(typeof(int?), result.SystemType);
        }

        [TestMethod]
        public void BigIntToLong()
        {
            var column = new Column { ColumnType = "BigInt" };
            var result = mapper.DBTypeToDataType(column);

            Assert.AreEqual("BigInt", result.DbType);
            Assert.AreEqual(typeof(long?), result.SystemType);
        }

        [TestMethod]
        public void FloatToDouble()
        {
            var column = new Column { ColumnType = "float" };
            var result = mapper.DBTypeToDataType(column);

            Assert.AreEqual("float", result.DbType);
            Assert.AreEqual(typeof(double?), result.SystemType);
        }

        [TestMethod]
        public void RealToFloat()
        {
            var column = new Column { ColumnType = "Real" };
            var result = mapper.DBTypeToDataType(column);

            Assert.AreEqual("Real", result.DbType);
            Assert.AreEqual(typeof(float?), result.SystemType);
        }

        [TestMethod]
        public void DateToDateTime()
        {
            var column = new Column { ColumnType = "Date" };
            var result = mapper.DBTypeToDataType(column);

            Assert.AreEqual("Date", result.DbType);
            Assert.AreEqual(typeof(DateTime?), result.SystemType);
        }


        [TestMethod]
        public void SmallDateTimeToDateTime()
        {
            var column = new Column { ColumnType = "SmallDateTime" };
            var result = mapper.DBTypeToDataType(column);

            Assert.AreEqual("SmallDateTime", result.DbType);
            Assert.AreEqual(typeof(DateTime?), result.SystemType);
        }


        [TestMethod]
        public void DateTimeToDateTime()
        {
            var column = new Column { ColumnType = "DateTime" };
            var result = mapper.DBTypeToDataType(column);

            Assert.AreEqual("DateTime", result.DbType);
            Assert.AreEqual(typeof(DateTime?), result.SystemType);
        }

        [TestMethod]
        public void DateTime2ToDateTime()
        {
            var column = new Column { ColumnType = "DateTime2" };
            var result = mapper.DBTypeToDataType(column);

            Assert.AreEqual("DateTime2", result.DbType);
            Assert.AreEqual(typeof(DateTime?), result.SystemType);
        }

        [TestMethod]
        public void DateTimeOffsetToDateTimeOffset()
        {
            var column = new Column { ColumnType = "DateTimeOffset" };
            var result = mapper.DBTypeToDataType(column);

            Assert.AreEqual("DateTimeOffset", result.DbType);
            Assert.AreEqual(typeof(DateTimeOffset?), result.SystemType);
        }

        [TestMethod]
        public void TimeStampToByteArray()
        {
            var column = new Column { ColumnType = "TimeStamp" };
            var result = mapper.DBTypeToDataType(column);

            Assert.AreEqual("TimeStamp", result.DbType);
            Assert.AreEqual(typeof(byte[]), result.SystemType);
        }

        [TestMethod]
        public void ImageToByteArray()
        {
            var column = new Column { ColumnType = "Image" };
            var result = mapper.DBTypeToDataType(column);

            Assert.AreEqual("Image", result.DbType);
            Assert.AreEqual(typeof(byte[]), result.SystemType);
        }

        [TestMethod]
        public void VarBinaryToByteArray()
        {
            var column = new Column { ColumnType = "VarBinary" };
            var result = mapper.DBTypeToDataType(column);

            Assert.AreEqual("VarBinary", result.DbType);
            Assert.AreEqual(typeof(byte[]), result.SystemType);
        }

        [TestMethod]
        public void BinaryToByteArray()
        {
            var column = new Column { ColumnType = "Binary" };
            var result = mapper.DBTypeToDataType(column);

            Assert.AreEqual("Binary", result.DbType);
            Assert.AreEqual(typeof(byte[]), result.SystemType);
        }

        [TestMethod]
        public void TimeToTimeSpan()
        {
            var column = new Column { ColumnType = "Time" };
            var result = mapper.DBTypeToDataType(column);

            Assert.AreEqual("Time", result.DbType);
            Assert.AreEqual(typeof(TimeSpan?), result.SystemType);
        }

        [TestMethod]
        public void BitToTimeSpan()
        {
            var column = new Column { ColumnType = "Bit" };
            var result = mapper.DBTypeToDataType(column);

            Assert.AreEqual("Bit", result.DbType);
            Assert.AreEqual(typeof(bool?), result.SystemType);
        }

        [TestMethod]
        public void OtherSetsToObject()
        {
            var column = new Column { ColumnType = "notFound" };
            var result = mapper.DBTypeToDataType(column);

            Assert.AreEqual("notFound", result.DbType);
            Assert.AreEqual(typeof(object), result.SystemType);
        }

        [TestMethod]
        public void NullableInDbTypeName()
        {
            var column = new Column { ColumnType = "Bit", ColumnIsNullable = true };
            var result = mapper.DBTypeToDataType(column);

            Assert.AreEqual("Bit(nullable)", result.DbType);
            Assert.AreEqual(typeof(bool?), result.SystemType);
        }

        [TestMethod]
        public void SystemTypeNoNullWhenPKIdentity()
        {
            var column = new Column { ColumnType = "Bit", PKIsIdentity = true };
            var result = mapper.DBTypeToDataType(column);

            Assert.AreEqual("Bit", result.DbType);
            Assert.AreEqual(typeof(bool), result.SystemType);
        }
    }
}
