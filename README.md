# AutoPocoIO

[![Official Site](https://img.shields.io/badge/site-autopoco.io-blue.svg)](http://autopoco.io) [![License LGPLv3](https://img.shields.io/badge/license-LGPLv3-green.svg)](http://www.gnu.org/licenses/lgpl-3.0.html)

## Overview

## Installation - ASP.NET
AutoPocoIO is available as a NuGet package. You can install it using the NuGet Package Console window:

```
PM> Install-Package AutoPocoIO
```
After installation in ASP.NET, update your existing [OWIN Startup](http://www.asp.net/aspnet/overview/owin-and-katana/owin-startup-class-detection) file with the following lines of code to set.

```csharp
public void Configuration(IAppBuilder app)
{
    ServiceCollection services = new ServiceCollection();
    services.AddAutoPoco()
             .ConfigureSqlServerApplicationDatabase("<connection string or its name>")
             .WithSqlServerResources();

     var containerBuilder = new ContainerBuilder();
     containerBuilder.Populate(services);
     var container = containerBuilder.Build();

     var config = new HttpConfiguration
     {
        //Set Autofac as dependency resolver
        DependencyResolver = new AutofacWebApiDependencyResolver(container)
     };
    app.UseAutofacLifetimeScopeInjector(container);
    app.UseAutofacWebApi(config);

    app.UseAutoPoco(config);

    app.UseWebApi(config); 
}
```
## Installation - ASP.NET Core
AutoPocoIO is available as a NuGet package. You can install it using the NuGet Package Console window:

```
PM> Install-Package AutoPocoIO
```
 In the `ConfigureServices` method of `Startup.cs`, register the AutoPoco, set up application database, and register resource providers.

```csharp
using AutoPocoIO.Extensions
```

```csharp
services.AddAutoPoco()
        .ConfigureSqlServerApplicationDatabase("<connection string or its name>")
        .WithSqlServerResources();
services.AddMvc();
```
In the `Configure` method, insert middleware for logging, dashboard, and swagger api explorer

```csharp
app.UseAutoPoco();
```

# Components #
## Providers ##
|Package|Description|
|---------|-----------|
|AutoPocoIO.MsSql|Includes extensions to setup MS SqlServer as the appliction database as well as connecting to other MS SqlServer databases|
