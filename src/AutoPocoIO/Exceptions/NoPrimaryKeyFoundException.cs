using System;

namespace AutoPocoIO.Exceptions
{
    [Serializable]
    public class NoPrimaryKeyFoundException : BaseCaughtException
    {
        private readonly string _entityName;
        private readonly string _baseTable;
        public NoPrimaryKeyFoundException(string entityName, string baseTable) : base()
        {
            _entityName = entityName;
            _baseTable = baseTable;
        }
        public override string Message => $"Virtual Entity '{_entityName}' references {_baseTable} and does not contain a primary key.";

        public override string HttpErrorMessage => "EntityKeyNotFound";

        protected NoPrimaryKeyFoundException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
        {
            throw new NotImplementedException();
        }
    }
}
