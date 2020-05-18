using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.Models;

namespace AutoPocoIO.DynamicSchema.Db
{
    public interface ISchemaInitializer
    {
        void Initilize();
        void ConfigureAction(Connector connector, OperationType dbAction);
    }
}
