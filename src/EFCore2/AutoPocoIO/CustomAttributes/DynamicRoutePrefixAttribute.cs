using AutoPocoIO.Extensions;
#if NETFULL
using System.Web.Http;
#endif

namespace AutoPocoIO.CustomAttributes
{
    /// <summary>
    /// Prepend route prefix with dashboard prefix
    /// </summary>
    public class DynamicRoutePrefixAttribute : RoutePrefixAttribute
    {
        private readonly string _prefix;
        /// <summary>
        /// Sets prefix to "DashboardRoute/<paramref name="prefix"/>".
        /// </summary>
        /// <param name="prefix">Route Prefix</param>
        public DynamicRoutePrefixAttribute(string prefix) : base(prefix)
        {
            _prefix = prefix;
        }

        /// <summary>
        /// Route prefix with dashboard path prepended.
        /// </summary>
        public override string Prefix
        {
            get
            {
                return string.IsNullOrEmpty(AutoPocoConfiguration.DashboardPathPrefix) ?
                                _prefix :
                                AutoPocoConfiguration.DashboardPathPrefix + "/" + _prefix;
            }
        }
    }
}
