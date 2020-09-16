using AutoPocoIO.DynamicSchema.Models;

namespace AutoPocoIO.DynamicSchema.Util
{
    /// <summary>
    /// Convert Database type to C# type.
    /// </summary>
    public interface IDbTypeMapper
    {
        /// <summary>
        /// Convert Database type to C# type. 
        /// All Types are nullable to allow blank data to raise a validation exception.
        /// PKs that are identity columns will remain non null to pass validation.
        /// </summary>
        /// <param name="column">The column to set system type</param>
        /// <returns>An object with both database type and system types.</returns>
        DataType DBTypeToDataType(Column column);
    }
}
