using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.Models;

namespace AutoPocoIO.DynamicSchema.Db
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISchemaInitializer
    {
        /// <summary>
        /// 
        /// </summary>
        void Initilize();
        /// <summary>
        /// 
        /// </summary>
        void FindSchemas();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connector"></param>
        /// <param name="dbAction"></param>
        void ConfigureAction(Connector connector, OperationType dbAction);
    }
}
