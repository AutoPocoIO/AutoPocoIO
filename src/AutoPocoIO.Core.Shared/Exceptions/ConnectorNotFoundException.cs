using System;

namespace AutoPocoIO.Exceptions
{
    /// <summary>
    /// Exception that sets status to 400 (Bad Request)
    /// </summary>
    public class ConnectorNotFoundException : BaseCaughtException
    {
        private readonly string connectorName;
        /// <summary>
        /// Initialize exception with connector name
        /// </summary>
        /// <param name="name">Connector name</param>
        public ConnectorNotFoundException(string name) : base(System.Net.HttpStatusCode.BadRequest)
        {
            connectorName = $"'{name}'";
        }

        /// <summary>
        /// Initialize exception with connector id
        /// </summary>
        /// <param name="id">Connector id</param>
        public ConnectorNotFoundException(Guid id) : base(System.Net.HttpStatusCode.BadRequest)
        {
            connectorName = $"with Id '{id}'";
        }

        /// <inheritdoc/>
        public override string Message => $"Connector {connectorName} not found.";
        /// <summary>
        /// Status message is <c>ConnectorNotFound</c>
        /// </summary>
        public override string HttpErrorMessage => "ConnectorNotFound";
    }
}