using AutoPocoIO.DynamicSchema.Db;
using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.Models;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoPocoIO.Resources
{
    /// <summary>
    /// Dynamic database access.
    /// </summary>
    public interface IOperationResource 
    {
        /// <summary>
        /// Schema definition.
        /// </summary>
        IDbSchema DbSchema { get; }
        /// <summary>
        /// Request configuration.
        /// </summary>
        Config Config { get; }
        /// <summary>
        /// Request database connector.
        /// </summary>
        Connector Connector { get; }
        /// <summary>
        /// Requested database.
        /// </summary>
        string DatabaseName { get; }
        /// <summary>
        /// Requested database object.
        /// </summary>
        string DbObjectName { get; }
        /// <summary>
        /// Requested schema.
        /// </summary>
        string SchemaName { get; }
        /// <summary>
        /// Database type.
        /// </summary>
        ResourceType ResourceType { get; }
        /// <summary>
        /// Register resouce specific services.
        /// </summary>
        /// <param name="serviceCollection">Resource service collection.</param>
        /// <param name="rootProvider">Application service provider.</param>
        void ApplyServices(IServiceCollection serviceCollection, IServiceProvider rootProvider);

        /// <summary>
        /// Configure resource operation
        /// </summary>
        /// <param name="connector">Request connector</param>
        /// <param name="dbAction">Operations type (read, write, delete)</param>
        /// <param name="dbObjectName">Database object to be accessed.</param>
        void ConfigureAction(Connector connector, OperationType dbAction, string dbObjectName);

        /// <summary>
        /// Insert a recored into a given table.
        /// </summary>
        /// <typeparam name="TViewModel">Type of view model</typeparam>
        /// <param name="value">Object to insert into <see cref="DbObjectName"/> </param>
        /// <returns>An instance of the object inserted.</returns>
        TViewModel CreateNewResourceRecord<TViewModel>(TViewModel value);
        /// <summary>
        /// Insert a recored into a given table
        /// </summary>
        /// <param name="value">Object to insert into <see cref="DbObjectName"/> </param>
        /// <returns></returns>
        object CreateNewResourceRecord(JToken value);
        /// <summary>
        /// Remove record from a given table
        /// </summary>
        /// <param name="keys">Primary Key(s)</param>
        /// <returns></returns>
        object DeleteResourceRecordById(string keys);

        /// <summary>
        /// Execute the configured stored procedure
        /// </summary>
        /// <param name="parameterDictionary">Key/Value pair of input and output parameters</param>
        /// <returns></returns>
        IDictionary<string, object> ExecuteProc(IDictionary<string, object> parameterDictionary);
        /// <summary>
        /// Retrieves a single record from a table by Primary Key.
        /// </summary>
        /// <param name="keys">The primary key value of the record to be retrieved as a string. 
        /// For composite keys, use semicolon separated string</param>
        /// <returns></returns>
        object GetResourceRecordById(string keys);
        /// <summary>
        /// Retrieves a single record from a table by Primary Key.
        /// </summary>
        /// <param name="keys">The primary key value of the record to be retrieved as a string. 
        /// For composite keys, use semicolon separated string</param>
        /// <param name="queryString">Request query string to apply odata filters.</param>
        /// <returns></returns>
        TViewModel GetResourceRecordById<TViewModel>(string keys, IDictionary<string, string> queryString);
        /// <summary>
        /// Retrieves all records in a table
        /// </summary>
        /// <param name="queryString">Request query string to apply odata filters.</param>
        /// <returns></returns>
        IQueryable<object> GetResourceRecords(IDictionary<string, string> queryString);
        /// <summary>
        /// Retrieves all records in a view
        /// </summary>
        /// <returns></returns>
        IQueryable<object> GetViewRecords();
        /// <summary>
        /// Initialize database schema for a specific object
        /// </summary>

        void LoadDbAdapter();
        /// <summary>
        /// Update a single record from a table by Primary Key.
        /// </summary>
        /// <param name="value">JSON object to update.</param>
        /// <param name="keys">The primary key value of the record to be retrieved as a string. 
        /// For composite keys, use semicolon separated string</param>
        /// <returns></returns>
        object UpdateResourceRecordById(JToken value, string keys);
        /// <summary>
        /// Update a single record from a table by Primary Key.
        /// </summary>
        /// <typeparam name="TViewModel">Model of the table columns</typeparam>
        /// <param name="value">Column values to update.</param>
        /// <param name="keys">The primary key value of the record to be retrieved as a string. 
        /// For composite keys, use semicolon separated string</param>
        /// <returns></returns>
        TViewModel UpdateResourceRecordById<TViewModel>(TViewModel value, string keys) where TViewModel : class;
        /// <summary>
        /// View column attirbutes
        /// </summary>
        /// <param name="columnName">Name of the column in the database.</param>
        /// <returns></returns>
        ColumnDefinition GetColumnDefinition(string columnName);
        /// <summary>
        /// List all database objects
        /// </summary>
        /// <returns>A list for each object type (Table, View, Stored Procedure)</returns>
        SchemaDefinition GetSchemaDefinition();
        /// <summary>
        /// List all parameters
        /// </summary>
        /// <returns></returns>
        StoredProcedureDefinition GetStoredProcedureDefinition();
        /// <summary>
        ///  Get a single parameter
        /// </summary>
        /// <param name="parameterName">Parameters name in the database</param>
        /// <returns></returns>
        StoredProcedureParameterDefinition GetStoredProcedureDefinition(string parameterName);
        /// <summary>
        /// List of columns that exists in a given table
        /// </summary>
        /// <returns></returns>
        TableDefinition GetTableDefinition();
        /// <summary>
        /// Load full schema
        /// </summary>

        void LoadSchema();
        /// <summary>
        /// Load schema for a specific stored procedure
        /// </summary>
        void LoadProc();
    }
}