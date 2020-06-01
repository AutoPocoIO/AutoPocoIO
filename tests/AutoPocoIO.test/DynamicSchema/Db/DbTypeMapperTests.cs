using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.DynamicSchema.Util;
using Xunit;
using System;

namespace AutoPocoIO.test.DynamicSchema.Db
{
    
     [Trait("Category", TestCategories.Unit)]
    public class DbTypeMapperTests
    {
        private readonly IDbTypeMapper mapper;

        public DbTypeMapperTests()
        {
            mapper = new DbTypeMapper();
        }

        [FactWithName]
        public void UniqueIdentifierToGuid()
        {
            var column = new Column { ColumnType = "uniqueIdentifier" };
            var result = mapper.DBTypeToDataType(column);

            Assert.Equal("uniqueIdentifier", result.DbType);
            Assert.Equal(typeof(Guid?), result.SystemType);
        }

        [FactWithName]
        public void XMLToString()
        {
            var column = new Column { ColumnType = "xml" };
            var result = mapper.DBTypeToDataType(column);

            Assert.Equal("xml", result.DbType);
            Assert.Equal(typeof(string), result.SystemType);
        }

        [FactWithName]
        public void NvarcharToString()
        {
            var column = new Column { ColumnType = "nvarchar" };
            var result = mapper.DBTypeToDataType(column);

            Assert.Equal("nvarchar", result.DbType);
            Assert.Equal(typeof(string), result.SystemType);
        }

        [FactWithName]
        public void CharToString()
        {
            var column = new Column { ColumnType = "char" };
            var result = mapper.DBTypeToDataType(column);

            Assert.Equal("char", result.DbType);
            Assert.Equal(typeof(string), result.SystemType);
        }

        [FactWithName]
        public void NCharToString()
        {
            var column = new Column { ColumnType = "nchar" };
            var result = mapper.DBTypeToDataType(column);

            Assert.Equal("nchar", result.DbType);
            Assert.Equal(typeof(string), result.SystemType);
        }

        [FactWithName]
        public void TextToString()
        {
            var column = new Column { ColumnType = "text" };
            var result = mapper.DBTypeToDataType(column);

            Assert.Equal("text", result.DbType);
            Assert.Equal(typeof(string), result.SystemType);
        }

        [FactWithName]
        public void VarCharToString()
        {
            var column = new Column { ColumnType = "VarChar" };
            var result = mapper.DBTypeToDataType(column);

            Assert.Equal("VarChar", result.DbType);
            Assert.Equal(typeof(string), result.SystemType);
        }

        [FactWithName]
        public void VarChar2ToString()
        {
            var column = new Column { ColumnType = "VarChar2" };
            var result = mapper.DBTypeToDataType(column);

            Assert.Equal("VarChar2", result.DbType);
            Assert.Equal(typeof(string), result.SystemType);
        }

        [FactWithName]
        public void SysNameToString()
        {
            var column = new Column { ColumnType = "sysName" };
            var result = mapper.DBTypeToDataType(column);

            Assert.Equal("sysName", result.DbType);
            Assert.Equal(typeof(string), result.SystemType);
        }

        [FactWithName]
        public void MoneyToDecimal()
        {
            var column = new Column { ColumnType = "Money" };
            var result = mapper.DBTypeToDataType(column);

            Assert.Equal("Money", result.DbType);
            Assert.Equal(typeof(decimal?), result.SystemType);
        }

        [FactWithName]
        public void SmallMoneyDecimal()
        {
            var column = new Column { ColumnType = "SmallMoney" };
            var result = mapper.DBTypeToDataType(column);

            Assert.Equal("SmallMoney", result.DbType);
            Assert.Equal(typeof(decimal?), result.SystemType);
        }

        [FactWithName]
        public void NumericToString()
        {
            var column = new Column { ColumnType = "Numeric" };
            var result = mapper.DBTypeToDataType(column);

            Assert.Equal("Numeric", result.DbType);
            Assert.Equal(typeof(decimal?), result.SystemType);
        }

        [FactWithName]
        public void DecimalToDecimal()
        {
            var column = new Column { ColumnType = "Decimal" };
            var result = mapper.DBTypeToDataType(column);

            Assert.Equal("Decimal", result.DbType);
            Assert.Equal(typeof(decimal?), result.SystemType);
        }

        [FactWithName]
        public void NumberToDecimal()
        {
            var column = new Column { ColumnType = "Number" };
            var result = mapper.DBTypeToDataType(column);

            Assert.Equal("Number", result.DbType);
            Assert.Equal(typeof(decimal?), result.SystemType);
        }

        [FactWithName]
        public void TinyIntToByte()
        {
            var column = new Column { ColumnType = "TinyInt" };
            var result = mapper.DBTypeToDataType(column);

            Assert.Equal("TinyInt", result.DbType);
            Assert.Equal(typeof(byte?), result.SystemType);
        }

        [FactWithName]
        public void SmallIntToShort()
        {
            var column = new Column { ColumnType = "SmallInt" };
            var result = mapper.DBTypeToDataType(column);

            Assert.Equal("SmallInt", result.DbType);
            Assert.Equal(typeof(short?), result.SystemType);
        }

        [FactWithName]
        public void IntToInt()
        {
            var column = new Column { ColumnType = "Int" };
            var result = mapper.DBTypeToDataType(column);

            Assert.Equal("Int", result.DbType);
            Assert.Equal(typeof(int?), result.SystemType);
        }

        [FactWithName]
        public void BigIntToLong()
        {
            var column = new Column { ColumnType = "BigInt" };
            var result = mapper.DBTypeToDataType(column);

            Assert.Equal("BigInt", result.DbType);
            Assert.Equal(typeof(long?), result.SystemType);
        }

        [FactWithName]
        public void FloatToDouble()
        {
            var column = new Column { ColumnType = "float" };
            var result = mapper.DBTypeToDataType(column);

            Assert.Equal("float", result.DbType);
            Assert.Equal(typeof(double?), result.SystemType);
        }

        [FactWithName]
        public void RealToFloat()
        {
            var column = new Column { ColumnType = "Real" };
            var result = mapper.DBTypeToDataType(column);

            Assert.Equal("Real", result.DbType);
            Assert.Equal(typeof(float?), result.SystemType);
        }

        [FactWithName]
        public void DateToDateTime()
        {
            var column = new Column { ColumnType = "Date" };
            var result = mapper.DBTypeToDataType(column);

            Assert.Equal("Date", result.DbType);
            Assert.Equal(typeof(DateTime?), result.SystemType);
        }


        [FactWithName]
        public void SmallDateTimeToDateTime()
        {
            var column = new Column { ColumnType = "SmallDateTime" };
            var result = mapper.DBTypeToDataType(column);

            Assert.Equal("SmallDateTime", result.DbType);
            Assert.Equal(typeof(DateTime?), result.SystemType);
        }


        [FactWithName]
        public void DateTimeToDateTime()
        {
            var column = new Column { ColumnType = "DateTime" };
            var result = mapper.DBTypeToDataType(column);

            Assert.Equal("DateTime", result.DbType);
            Assert.Equal(typeof(DateTime?), result.SystemType);
        }

        [FactWithName]
        public void DateTime2ToDateTime()
        {
            var column = new Column { ColumnType = "DateTime2" };
            var result = mapper.DBTypeToDataType(column);

            Assert.Equal("DateTime2", result.DbType);
            Assert.Equal(typeof(DateTime?), result.SystemType);
        }

        [FactWithName]
        public void DateTimeOffsetToDateTimeOffset()
        {
            var column = new Column { ColumnType = "DateTimeOffset" };
            var result = mapper.DBTypeToDataType(column);

            Assert.Equal("DateTimeOffset", result.DbType);
            Assert.Equal(typeof(DateTimeOffset?), result.SystemType);
        }

        [FactWithName]
        public void TimeStampToByteArray()
        {
            var column = new Column { ColumnType = "TimeStamp" };
            var result = mapper.DBTypeToDataType(column);

            Assert.Equal("TimeStamp", result.DbType);
            Assert.Equal(typeof(byte[]), result.SystemType);
        }

        [FactWithName]
        public void ImageToByteArray()
        {
            var column = new Column { ColumnType = "Image" };
            var result = mapper.DBTypeToDataType(column);

            Assert.Equal("Image", result.DbType);
            Assert.Equal(typeof(byte[]), result.SystemType);
        }

        [FactWithName]
        public void VarBinaryToByteArray()
        {
            var column = new Column { ColumnType = "VarBinary" };
            var result = mapper.DBTypeToDataType(column);

            Assert.Equal("VarBinary", result.DbType);
            Assert.Equal(typeof(byte[]), result.SystemType);
        }

        [FactWithName]
        public void BinaryToByteArray()
        {
            var column = new Column { ColumnType = "Binary" };
            var result = mapper.DBTypeToDataType(column);

            Assert.Equal("Binary", result.DbType);
            Assert.Equal(typeof(byte[]), result.SystemType);
        }

        [FactWithName]
        public void TimeToTimeSpan()
        {
            var column = new Column { ColumnType = "Time" };
            var result = mapper.DBTypeToDataType(column);

            Assert.Equal("Time", result.DbType);
            Assert.Equal(typeof(TimeSpan?), result.SystemType);
        }

        [FactWithName]
        public void BitToTimeSpan()
        {
            var column = new Column { ColumnType = "Bit" };
            var result = mapper.DBTypeToDataType(column);

            Assert.Equal("Bit", result.DbType);
            Assert.Equal(typeof(bool?), result.SystemType);
        }

        [FactWithName]
        public void OtherSetsToObject()
        {
            var column = new Column { ColumnType = "notFound" };
            var result = mapper.DBTypeToDataType(column);

            Assert.Equal("notFound", result.DbType);
            Assert.Equal(typeof(object), result.SystemType);
        }

        [FactWithName]
        public void NullableInDbTypeName()
        {
            var column = new Column { ColumnType = "Bit", ColumnIsNullable = true };
            var result = mapper.DBTypeToDataType(column);

            Assert.Equal("Bit(nullable)", result.DbType);
            Assert.Equal(typeof(bool?), result.SystemType);
        }

        [FactWithName]
        public void SystemTypeNoNullWhenPKIdentity()
        {
            var column = new Column { ColumnType = "Bit", PKIsIdentity = true };
            var result = mapper.DBTypeToDataType(column);

            Assert.Equal("Bit", result.DbType);
            Assert.Equal(typeof(bool), result.SystemType);
        }
    }
}
