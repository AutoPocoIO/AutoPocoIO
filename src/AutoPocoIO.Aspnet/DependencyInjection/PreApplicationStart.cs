using Microsoft.Web.Infrastructure.DynamicModuleHelper;

namespace AutoPoco.DependencyInjection
{
    public static class PreApplicationStart
    {
        private static bool _startWasCalled;

        public static void Start()
        {
            if (_startWasCalled) return;

            _startWasCalled = true;
            DynamicModuleUtility.RegisterModule(typeof(RequestScopeManagement));
        }
    }
}
