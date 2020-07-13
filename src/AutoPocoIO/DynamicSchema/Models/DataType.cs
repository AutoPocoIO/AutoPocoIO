using System;
using System.Globalization;

namespace AutoPocoIO.DynamicSchema.Models
{
    /// <summary>
    /// Combine the Database type and the C# type
    /// </summary>
    public struct DataType : IEquatable<DataType>
    {
        /// <summary>
        /// Database Type
        /// </summary>
        public string DbType { get; set; }
        /// <summary>
        /// C# Type
        /// </summary>
        public Type SystemType { get; set; }

        /// <summary>
        /// Display both types
        /// </summary>
        /// <returns> A string that represents the current object.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "({0}) {1}", DbType, SystemType.Name);
        }

        ///<inheritdoc/>
        public override bool Equals(object obj)
        {
            if (!(obj is DataType))
                return false;

            return Equals((DataType)obj);

        }

        ///<inheritdoc/>
        public override int GetHashCode()
        {
            return (DbType.GetHashCode() * 17) ^ SystemType.GetHashCode();
        }

        ///<inheritdoc/>
        public static bool operator ==(DataType left, DataType right)
        {
            return left.Equals(right);
        }

        ///<inheritdoc/>
        public static bool operator !=(DataType left, DataType right)
        {
            return !(left == right);
        }

        ///<inheritdoc/>
        public bool Equals(DataType other)
        {
            return DbType.Equals(other.DbType, StringComparison.InvariantCultureIgnoreCase)
                    && SystemType == other.SystemType;
        }
    }
}
