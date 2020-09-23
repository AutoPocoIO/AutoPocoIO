namespace AutoPocoIO.Exceptions
{
    internal class EntityValidationException : BaseCaughtException
    {
        private readonly string _message;
        public EntityValidationException(string message) : base()
        {
            _message = message;
        }

        public override string Message => _message;
    }
}
