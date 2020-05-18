namespace AutoPocoIO.Exceptions
{
    internal class OpenConnectorException : BaseCaughtException
    {
        private readonly string _connector;

        public OpenConnectorException(string connector) : base()
        {
            _connector = connector;
        }
        public override string Message => $"An error occurred opening the connector '{_connector}'.";
        public override string HttpErrorMessage => "ConnectionError";
    }
}
