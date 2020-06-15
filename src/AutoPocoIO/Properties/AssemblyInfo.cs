using System.Web;

#if NETFULL
[assembly: PreApplicationStartMethod(typeof(AutoPoco.DependencyInjection.PreApplicationStart), nameof(AutoPoco.DependencyInjection.PreApplicationStart.Start))]
#endif