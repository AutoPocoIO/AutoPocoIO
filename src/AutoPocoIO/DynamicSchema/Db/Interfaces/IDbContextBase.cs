using Microsoft.EntityFrameworkCore.Metadata;

namespace AutoPocoIO.DynamicSchema.Db
{
    /// <summary>
    /// A DbContext instance represents a session with the database and can be used to
    ///     query and save instances of your entities. DbContext is a combination of the
    ///     Unit Of Work and Repository patterns.
    /// </summary>
    public interface IDbContextBase
    {
        /// <summary>
        ///  Saves all changes made in this context to the database.
        /// </summary>
        /// <returns>The number of state entries written to the database.</returns>
        int SaveChanges();
        /// <summary>
        /// The metadata about the shape of entities, the relationships between them, and how they map to the database.
        /// </summary>
        IModel Model { get; }
    }
}