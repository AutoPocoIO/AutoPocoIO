namespace AutoPocoIO.Exceptions
{
    public class ConnectorNotFoundException : BaseCaughtException
    {
        private readonly string connectorName;
        public ConnectorNotFoundException(string name) : base(System.Net.HttpStatusCode.BadRequest)
        {
            connectorName = $"'{name}'";
        }

        public ConnectorNotFoundException(int id) : base(System.Net.HttpStatusCode.BadRequest)
        {
            connectorName = $"with Id '{id}'";
        }
        public override string Message => $"Connector {connectorName} not found.";
        public override string HttpErrorMessage => "ConnectorNotFound";
    }
}