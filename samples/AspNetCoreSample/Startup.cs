using AspNetCoreSample.Migration;
using AutoPocoIO.Api;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.Extensions;
using AutoPocoIO.Factories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq.Expressions;

namespace AspNetCoreSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAutoPoco()
                    .ConfigureSqlServerApplicationDatabase(Configuration.GetConnectionString("AppDb"))
                    .WithSqlServerResources();
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            app.UseAutoPoco();

            app.UseStaticFiles();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Order}/{action=Index}/{id?}");
            });


            //Migrate sample database
            var migrator = new Migrator(Configuration.GetConnectionString("AppDb"));
            migrator.Migrate();

            //Try Seed sampleDb connection
            var tableOp = app.ApplicationServices.GetRequiredService<ITableOperations>();

            var connection = new
            {
                Id = Guid.NewGuid().ToString(),
                Name = "sampleSales",
                ResourceType = 1,
                Schema = "sales",
                ConnectionString = @"Data Source=""(localdb)\MSSQLLocalDB"";Initial Catalog=autopocoCore;Persist Security Info=False;User ID=;Password=;MultipleActiveResultSets=False;Connect Timeout=30;TrustServerCertificate=False",
                RecordLimit = 500,
                InitialCatalog = "autopocoCore",
                DataSource = @"(localdb)\MSSQLLocalDB",
                IsActive = true
            };

            try
            {
                tableOp.CreateNewRow("appDb", "connector", connection);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException) { }

        }
    }
}
