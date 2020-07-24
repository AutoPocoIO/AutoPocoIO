using AspNetCoreSample.Migration;
using AutoPocoIO.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin;
using Owin;
using System.Configuration;
using System.Reflection;

[assembly: OwinStartup(typeof(AspNetSample.Startup))]

namespace AspNetSample
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ServiceCollection services = new ServiceCollection();
            services.AddAutoPoco()
                    .RegisterControllers(Assembly.GetExecutingAssembly())
                    .ConfigureSqlServerApplicationDatabase(ConfigurationManager.ConnectionStrings["AppDb"].ConnectionString)
                    .WithSqlServerResources();

            app.UseAutoPoco(services);




            //Migrate sample database
            var migrator = new SampleDataMigrator(ConfigurationManager.ConnectionStrings["AppDb"].ConnectionString);
            migrator.Migrate();
        }
    }
}
