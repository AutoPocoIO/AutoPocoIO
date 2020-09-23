using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.Resources;

namespace AutoPocoIO.Factories
{
    /// <summary>
    /// Search for a resouce by connector.
    /// </summary>
    public interface IResourceFactory
    {
        /// <summary>
        /// Get resouce by connector id while ignoring operation type.
        /// </summary>
        /// <param name="connectorId">Connector id.</param>
        /// <param name="dbObjectName">Database object used filter schema.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        IOperationResource GetResource(string connectorId, string dbObjectName);
        /// <summary>
        /// Get resouce by connector id.
        /// </summary>
        /// <param name="connectorId">Connector id.</param>
        /// <param name="dbAction">Current database interaction.</param>
        /// <param name="dbObjectName">Database object used filter schema.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        IOperationResource GetResourceById(string connectorId, OperationType dbAction, string dbObjectName);
        /// <summary>
        /// Get resouce by connector name.
        /// </summary>
        /// <param name="connectorName">Connector name.</param>
        /// <param name="dbAction">Current database interaction.</param>
        /// <param name="dbObjectName">Database object used filter schema.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        IOperationResource GetResource(string connectorName, OperationType dbAction, string dbObjectName);
    }
}
