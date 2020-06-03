using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.Resources;

namespace AutoPocoIO.Factories
{
    public interface IResourceFactory
    {
        IOperationResource GetResource(int connectorId, string dbObjectName);
        IOperationResource GetResource(int connectorId, OperationType dbAction, string dbObjectName);
        IOperationResource GetResource(string connectorName, OperationType dbAction, string dbObjectName);
    }
}
