using Microsoft.Web.Infrastructure.DynamicModuleHelper;

namespace AutoPoco.DependencyInjection
{
    /// <summary>
    /// Register IOC scope cleanup
    /// </summary>
    public static class PreApplicationStart
    {
        private static bool _startWasCalled;

        /// <summary>
        /// Register IOC scope cleanup
        /// </summary>
        public static void Start()
        {
            if (_startWasCalled) return;

            _startWasCalled = true;
            DynamicModuleUtility.RegisterModule(typeof(RequestScopeManagement));
        }
    }
}
