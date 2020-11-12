using AutoPocoIO.CustomAttributes;
using AutoPocoIO.Exceptions;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace AutoPocoIO.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class OdataExtensions
    {
        /// <summary>
        /// Apply the Odata query to the given IQueryable in the right order.
        /// </summary>
        /// <typeparam name="T">Query element type.</typeparam>
        /// <param name="query"> The original <see cref="IQueryable"/>.</param>
        /// <param name="recordLimit">Maxium number or records to return.</param>
        /// <param name="queryString">Http request query strings to parse for odata syntax.</param>
        /// <returns></returns>
        public static IQueryable<object> ApplyQuery<T>(this IQueryable<T> query, int recordLimit, IDictionary<string, string> queryString)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            if (queryString == null) throw new ArgumentNullException(nameof(queryString));

            var defaultOdataQuerySettings = new ODataQuerySettings();
            var settings = new ODataValidationSettings
            {
                MaxTop = recordLimit
            };

            //Build odata context 

            var modelBuilder = ODataQueryExtensions.ConfigureConventionBuilder();
            modelBuilder.AddEntityType(query.ElementType);
            modelBuilder.OnModelCreating += builder => AddCompoundKeyConvention(builder);

            var edmModel = modelBuilder.GetEdmModel();
            var odataPath = new ODataPath();
            var odataQueryConext = new ODataQueryContext(edmModel, query.ElementType, odataPath);

            var options = odataQueryConext.CreateOptionsFromQueryString(queryString);

            //set default record top limit
            if (options.Top == null && options.Count?.Value != true)
            {
                queryString.Add("$top", recordLimit.ToString(CultureInfo.InvariantCulture));
                options = odataQueryConext.CreateOptionsFromQueryString(queryString);
            }

            options.Validate(settings);

            IQueryable<dynamic> retList;
            if (options.OrderBy == null)
            {
                //Optimize odata filter ordering
                IQueryable results = options.ApplyTo(query, defaultOdataQuerySettings, AllowedQueryOptions.OrderBy);
                retList = results as IQueryable<dynamic>;
            }
            else
            {
                //Optimize odata filter ordering
                IQueryable results = options.OrderBy.ApplyTo(query, defaultOdataQuerySettings);
                results = options.ApplyTo(results, defaultOdataQuerySettings, AllowedQueryOptions.OrderBy);
                retList = results as IQueryable<dynamic>;
            }

            if (options.Count?.Value == true)
            {
                IList<dynamic> count = new List<dynamic>();
                CountTotal countObj = new CountTotal
                {
                    Count = retList.Count()
                };
                count.Add(countObj);
                return count.AsQueryable();
            }

            return retList;
        }

        private static void AddCompoundKeyConvention(ODataModelBuilder builder)
        {
            foreach (var entityType in builder.StructuralTypes)
            {
                IEnumerable<PropertyConfiguration> keys = entityType.Properties
                    .Where(p => p.PropertyInfo.CustomAttributes?.Any(c => c.AttributeType == typeof(CompoundPrimaryKeyAttribute)) == true)
                    .OrderBy(p => p.PropertyInfo.GetCustomAttribute<CompoundPrimaryKeyAttribute>().Order);

                foreach (var key in keys)
                {
                    if (entityType is EntityTypeConfiguration entity)
                    {
                        entity.HasKey(key.PropertyInfo);
                    }
                }
            }
        }
    }
}
