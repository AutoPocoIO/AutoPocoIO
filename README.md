# AutoPocoIO

[![Official Site](https://img.shields.io/badge/site-autopoco.io-blue.svg)](http://autopoco.io) [![License LGPLv3](https://img.shields.io/badge/license-LGPLv3-green.svg)](http://www.gnu.org/licenses/lgpl-3.0.html) [![Build status](https://ci.appveyor.com/api/projects/status/4qe1ffp77uaecy29/branch/master?svg=true&passingText=Master%20Passed&pendingText=Master%20Building&failingText=Master%20Failed)](https://ci.appveyor.com/project/autopocoio/autopocoio/branch/master) [![Build status](https://ci.appveyor.com/api/projects/status/4qe1ffp77uaecy29/branch/dev?svg=true&passingText=Dev%20Passed&pendingText=Dev%20Building&failingText=Dev%20Failed)](https://ci.appveyor.com/project/autopocoio/autopocoio/branch/dev) [![codecov](https://codecov.io/gh/AutoPocoIO/AutoPocoIO/branch/dev/graph/badge.svg)](https://codecov.io/gh/AutoPocoIO/AutoPocoIO)



## Overview
Auto generate Entity Framework POCO classes at runtime to build dynamic type ASP.NET applications. Classes are generated from your database's current schema. Supports CRUD and execute operations on SQL, mysql, & Oracle tables, views, and stored procedures. Requests are logged automatically, so there is no need to write custom logging. Grant or restrict column-level access using the Role Based authorization service.

Or use AutoPoco to expose your database via a REST Web API. Use OData to query and filter your requests. Interact with the Web API via Swagger.

* [AspNet Framwork](#aspnet)
* [AspNet Core](#aspnet-core)

### Installation
Install the provider package corresponding to your target database(s). 
```
dotnet add package AutoPocoIO.MsSql
```

### Usage
Inject Operation type into controller
```csharp
public SampleController(ITableOperations tableOps, ILoggingService loggingService)
{
    _loggingService = loggingService;
    _tableOps = tableOps;
}
```

Create and load an object from a Database Table or View:
```csharp
var foo = tableOp.GetAll("AdventureWorksDB", "Customer");
var bar = tableOp.GetById("AdventureWorksDB", "Customer", 42);
var foo1 = viewOp.GetAll("AdventureworksDB", "vw_Customer");
```
Call a Stored Procedure, with or without parameters:
```csharp
var foo = storedProcedureOp.ExecuteNoParameters("AdventureWorksDB", "sproc_Customers");
var bar = storedProcedureOp.Execute("AdventureWorksDB", "sproc_customer", "'id':1");
```
Automatically log the request by passing it to the operation:
```csharp
tableOp.CreateNewRow("AdventureWorksDB", "Customer", object, loggingService);
tableOp.UpdateRow("AdventureWorksDB", "Customer", 42, object, loggingService);
```

## Setup
### ASP.NET
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
### ASP.NET Core
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
