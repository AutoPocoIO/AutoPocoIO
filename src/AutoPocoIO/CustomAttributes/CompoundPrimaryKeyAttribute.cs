using System;

namespace AutoPocoIO.CustomAttributes
{
    /// <summary>
    /// Associates property with as a Primary key in a database table in OData
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class CompoundPrimaryKeyAttribute : Attribute
    {
        public CompoundPrimaryKeyAttribute(int order)
        {
            Order = order;
        }

        /// <summary>
        /// PK Order
        /// </summary>
        public int Order { get; }
    }
}