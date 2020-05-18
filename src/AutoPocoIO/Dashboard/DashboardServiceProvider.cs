using AutoPocoIO.Context;
using AutoPocoIO.Dashboard.Pages;
using AutoPocoIO.Dashboard.Repos;
using AutoPocoIO.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace AutoPocoIO.Dashboard
{
    internal class DashboardServiceProvider
    {
        private static IServiceProvider _provider;

        public DashboardServiceProvider() { }

        public static DashboardServiceProvider Instance { get; } = new DashboardServiceProvider();

        public virtual IServiceProvider GetServiceProvider(IServiceProvider rootProvider)
        {
            if (_provider == null)
            {
                var services = new ServiceCollection();

                services.AddSingleton<DashboardRoutes>();

                services.TryAddSingleton<ITimeProvider, DefaultTimeProvider>();

                services.AddScoped<AppDbContext>();
                services.AddScoped<LogDbContext>();

                var appOptions = rootProvider.GetRequiredService<DbContextOptions<AppDbContext>>();
                var logOptions = rootProvider.GetRequiredService<DbContextOptions<LogDbContext>>();
                services.AddScoped(c => appOptions);
                services.AddScoped(c => logOptions);

                /*
                //Resources
                services.AddTransient<IResourceFactory, ResourceFactory>();
                services.AddTransient<IAppAdminService, ProAppAdminService>();
                services.AddScoped<Config>();

                services.TryAddScoped<ILicenseService, LicenseService>();
               
#if NETCORE
                services.AddSingleton<Microsoft.AspNetCore.Http.IHttpContextAccessor, Microsoft.AspNetCore.Http.HttpContextAccessor>();
#endif

                services.AddScoped<IRequestQueryStringService, RequestQueryStringService>();
                services.AddScoped<IConnectionStringFactory, ConnectionStringFactory>();
      

                //Pages -------------------
                //AppKey
                services.AddScoped<ListApplicationKeyPage>();
                services.AddScoped<_ApplicationKeyGrid>();
                services.AddScoped<EditApplicationKeyPage>();
                services.AddScoped<_ApplicationKeyForm>();
                services.AddScoped<AddApplicationMemberPage>();
                services.AddScoped<_ApplicationMemberGrid>();
                //Connector
                services.AddScoped<AddConnectorPage>();
                services.AddScoped<ListConnectorsPage>();
                services.AddScoped<_ConnectorsGrid>();
                services.AddScoped<EditConnectorPage>();
                services.AddScoped<_ConnectorForm>();
                services.AddScoped<_JoinForm>();
                services.AddScoped<_JoinsGrid>();
                services.AddScoped<EditJoinPage>();
                //ManageData
                services.AddScoped<ViewDataPage>();
                services.AddScoped<ManageDataPage>();
                services.AddScoped<_ConnectorsListGrid>();
                services.AddScoped<ViewSchemaPage>();
                //Roles
                services.AddScoped<ListRolesPage>();
                services.AddScoped<_RolesGrid>();
                services.AddScoped<EditRolesPage>();
                services.AddScoped<_RoleForm>();

                services.AddScoped<EditPermissionPage>();
                services.AddScoped<_PermissionForm>();
                services.AddScoped<AddMembersPage>();
                services.AddScoped<_PermissionsGrid>();
                services.AddScoped<_MembersGrid>();
                //Shared
                //User
                services.AddScoped<ListUsersPage>();
                services.AddScoped<_UsersGrid>();
                services.AddScoped<EditUserPage>();
                services.AddScoped<_UserForm>();
                //Dashboard
                services.AddScoped<_RecentRequestsDataTable>();
                services.AddScoped<DashboardPage>();

                //Repos -------------------
                //AppKey
                services.AddScoped<IDashboardRepo<ApplicationKeyViewModel, Guid>, ApplicationKeyRepo>();
                services.AddScoped<IApplicationMemberRepo, ApplicationMemberRepo>();
                //Connector
                services.AddScoped<IDashboardRepo<ConnectorViewModel, int>, ConnectorRepo>();
                services.AddScoped<IUserJoinRepo, UserJoinRepo>();
                //ManageData
                services.AddScoped<IManageDataRepo, ManageDataRepo>();
                //Roles
                services.AddScoped<IDashboardRepo<RoleViewModel, string>, RoleRepo>();
                services.AddScoped<IPermissionsRepo, PermissionsRepo>();
                services.AddScoped<IUserRoleRepo, UserRoleRepo>();
                services.AddScoped<IPermissionsRepo, PermissionsRepo>();
                //Shared
                //User
                services.AddScoped<IDashboardRepo<UserViewModel, string>, UserRepo>();
                //Dashboard
                services.AddScoped<IRequestHistoryRepo, RequestHistoryRepo>();*/
                services.AddScoped<IDashboardRepo, DashboardRepo>();

                services.AddScoped<DashboardPage>();

                _provider = services.BuildServiceProvider();
            }

            return _provider;
        }
    }
}
