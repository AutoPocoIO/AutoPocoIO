using System;

namespace AutoPocoIO.CustomAttributes
{
    /// <summary>
    /// Flag for swagger to add Odata standard filters.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class UseOdataInSwaggerAttribute : Attribute
    {
    }
}