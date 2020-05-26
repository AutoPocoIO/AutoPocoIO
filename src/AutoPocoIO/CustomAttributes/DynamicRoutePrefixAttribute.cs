using AutoPocoIO.Extensions;
#if NETFULL
using System.Web.Http;
#endif

namespace AutoPocoIO.CustomAttributes
{
    internal class DynamicRoutePrefixAttribute : RoutePrefixAttribute
    {
        private readonly string _prefix;
        public DynamicRoutePrefixAttribute(string prefix) : base(prefix)
        {
            _prefix = prefix;
        }

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
