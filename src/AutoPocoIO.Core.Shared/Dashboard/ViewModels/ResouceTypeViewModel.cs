using System.Linq;

namespace AutoPocoIO.Dashboard.ViewModels
{
    public class ResouceTypeViewModel
    {
        public string ProviderName { get; set; }
        public string DisplayName => ProviderName.Split(',').Last();
    }
}
