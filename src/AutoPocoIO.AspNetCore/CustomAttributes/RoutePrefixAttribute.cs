using AutoPocoIO.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace AutoPocoIO.CustomAttributes
{
    /// <summary>
    /// Core workaround for no RoutePrefix Attribute
    /// </summary>
    public abstract class RoutePrefixAttribute : RouteAttribute
    {
        public RoutePrefixAttribute(string template)
            : base(SetUpPrefix() + template)
        {
        }

        public abstract string Prefix { get; }

        private static string SetUpPrefix()
        {
            return string.IsNullOrEmpty(AutoPocoConfiguration.DashboardPathPrefix) ?
                                 string.Empty :
                                 AutoPocoConfiguration.DashboardPathPrefix + "/";
        }

    }
}