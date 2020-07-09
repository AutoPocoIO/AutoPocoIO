using AutoPocoIO.Dashboard.ViewModels;
using System.Collections.Generic;

namespace AutoPocoIO.Dashboard.Repos
{
    /// <summary>
    /// CRUD operations for connectors in the dashboard
    /// </summary>
    public interface IConnectorRepo
    {
        /// <summary>
        /// Total number of registered connectors
        /// </summary>
        /// <returns></returns>
        int ConnectorCount();
        /// <summary>
        /// List of connector information
        /// </summary>
        /// <returns></returns>
        IEnumerable<ConnectorViewModel> ListConnectors();
        /// <summary>
        /// Update a connector
        /// </summary>
        /// <param name="model">Values to update connector.</param>
        /// <returns></returns>
        string Save(ConnectorViewModel model);
        /// <summary>
        /// Get a single connector
        /// </summary>
        /// <param name="id">Connector Id</param>
        /// <returns></returns>
        ConnectorViewModel GetById(string id);
        /// <summary>
        /// Insert a connector.
        /// </summary>
        /// <param name="model">Values to create a connector.</param>
        /// <returns></returns>
        string Insert(ConnectorViewModel model);
        /// <summary>
        /// Validate connector values on Insert/Update
        /// </summary>
        /// <param name="model">Values to validate</param>
        /// <param name="errors">Error dictionary to display</param>
        void Validate(ConnectorViewModel model, IDictionary<string, string> errors);
        /// <summary>
        /// Remove connector.
        /// </summary>
        /// <param name="id">Connector Id to remove.</param>
        void Delete(string id);
    }
}