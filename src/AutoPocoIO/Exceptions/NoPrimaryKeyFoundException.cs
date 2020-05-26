using System;

namespace AutoPocoIO.Exceptions
{
    [Serializable]
    public class NoPrimaryKeyFoundException : BaseCaughtException
    {
        private readonly string _entityName;
        public NoPrimaryKeyFoundException(string entityName) : base()
        {
            _entityName = entityName;
        }
        public override string Message => $"Entity '{_entityName}' does not contain a primary key.";

        public override string HttpErrorMessage => "EntityKeyNotFound";

        protected NoPrimaryKeyFoundException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
        {
            throw new NotImplementedException();
        }
    }
}
