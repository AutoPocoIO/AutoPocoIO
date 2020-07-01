using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.Resources;

namespace AutoPocoIO.Factories
{
    public interface IResourceFactory
    {
        IOperationResource GetResource(string connectorId, string dbObjectName);
        IOperationResource GetResourceById(string connectorId, OperationType dbAction, string dbObjectName);
        IOperationResource GetResource(string connectorName, OperationType dbAction, string dbObjectName);
    }
}
