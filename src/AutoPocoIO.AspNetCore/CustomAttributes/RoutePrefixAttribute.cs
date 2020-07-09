using AutoPocoIO.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace AutoPocoIO.CustomAttributes
{
    /// <summary>
    /// Core workaround for no RoutePrefix Attribute
    /// </summary>
    public abstract class RoutePrefixAttribute : RouteAttribute
    {
        /// <summary>
        /// Sets prefix to "DashboardRoute/<paramref name="template"/>".
        /// </summary>
        /// <param name="template">Route Prefix</param>
        public RoutePrefixAttribute(string template)
            : base(SetUpPrefix() + template)
        {
        }

        /// <summary>
        /// Route prefix
        /// </summary>
        public abstract string Prefix { get; }

        private static string SetUpPrefix()
        {
            return string.IsNullOrEmpty(AutoPocoConfiguration.DashboardPathPrefix) ?
                                 string.Empty :
                                 AutoPocoConfiguration.DashboardPathPrefix + "/";
        }

    }
}