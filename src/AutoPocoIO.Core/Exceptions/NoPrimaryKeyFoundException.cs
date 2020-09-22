namespace AutoPocoIO.Exceptions
{
    /// <summary>
    /// Exception that sets status to 500 (Sever Error)
    /// </summary>
    public class NoPrimaryKeyFoundException : BaseCaughtException
    {
        private readonly string _entityName;
        /// <summary>
        ///  Initialize exception with entity name
        /// </summary>
        /// <param name="entityName"></param>
        public NoPrimaryKeyFoundException(string entityName) : base()
        {
            _entityName = entityName;
        }
        /// <inheritdoc/>
        public override string Message => $"Entity '{_entityName}' does not contain a primary key.";
        /// <summary>
        /// Status message is <c>EntityKeyNotFound</c>
        /// </summary>
        public override string HttpErrorMessage => "EntityKeyNotFound";
    }
}
