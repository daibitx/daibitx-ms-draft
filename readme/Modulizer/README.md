# Daibitx.Extension.Modularize

English | [中文](./README.zh-CN.md)

[![NuGet](https://img.shields.io/nuget/v/Daibitx.Extension.Modularize.svg)](https://www.nuget.org/packages/Daibitx.Extension.Modularize)
 [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

**Daibitx.Extension.Modularize** is a modular application framework designed for ASP.NET Core. It provides a plugin-based architecture that enables your application to dynamically load and manage independent modules.

## ✨ Features

- 🔌 **Dynamic Module Discovery** – Automatically scans and loads modules from the specified directory
- 📦 **Module Isolation** – Each module runs in its own AssemblyLoadContext
- 🔄 **Dependency Injection Integration** – Fully integrates with the ASP.NET Core DI container
- ⚙️ **Configuration Management** – Supports module-level configuration files
- 🚀 **Lifecycle Management** – Complete workflow for module loading, initialization, and configuration
- 🔧 **Flexible Configuration** – Customize module paths and loading behavior

## 📦 Installation

```
dotnet add package Daibitx.Extension.Modularize
```

## 🚀 Getting Started

### 1. Basic Configuration

```
// Program.cs
using Daibitx.Extension.Modularize;

var builder = WebApplication.CreateBuilder(args);

// Configure modular services
builder.ConfigureModulesService(options =>
{
    // Set module directory
    options.ModulesPath = Path.Combine(AppContext.BaseDirectory, "Modules");
});

var app = builder.Build();

// Configure module pipeline
app.ConfigureModules(app, app, app.Services);

app.Run();
```

### 2. Creating a Module

#### 2.1 Create the Module Startup Class

```
// MyModule/Startup.cs
using Daibitx.Extension.Modularize.Abstractons;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

public class MyModuleStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        // Register module services
        services.AddScoped<IMyModuleService, MyModuleService>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes)
    {
        // Configure module routes
        routes.MapControllerRoute(
            name: "mymodule",
            pattern: "mymodule/{controller=Home}/{action=Index}/{id?}");
    }
}
```

#### 2.2 Create the Module Configuration File

```
// MyModule/MyModule.json
{
  "AssemblyName": "MyModule",
  "DllPath": "MyModule.dll",
  "ConfigPath": "MyModule.json",
  "AssemblyContextPath": "MyModule"
}
```

### 3. Module Directory Structure

```
YourApp/
├── Modules/
│   ├── MyModule/
│   │   ├── MyModule.dll          # Module assembly
│   │   ├── MyModule.json         # Module config file
│   └── AnotherModule/
│       ├── AnotherModule.dll
│       └── AnotherModule.json
├── YourApp.dll
└── appsettings.json
```

## 📋 API Reference

### HostServiceExtension Methods

#### ConfigureModulesService

Configure modular services.

```
public static void ConfigureModulesService(
    this WebApplicationBuilder application, 
    Action<ModuleOptions> moduleOptions
)
```

**Parameters:**

- `moduleOptions` – Delegate for configuring module options

**Example:**

```
builder.ConfigureModulesService(options =>
{
    options.ModulesPath = "./Modules";
});
```

#### ConfigureModules

Configure the module pipeline.

```
public static void ConfigureModules(
    this IApplicationBuilder builder, 
    IEndpointRouteBuilder routes, 
    IServiceProvider serviceProvider
)
```

**Parameters:**

- `builder` – Application builder
- `routes` – Route builder
- `serviceProvider` – Service provider

### ModuleOptions

```
public class ModuleOptions
{
    /// <summary>
    /// Path to the modules directory
    /// </summary>
    public string ModulesPath { get; set; }
    
    /// <summary>
    /// Loaded module list (internal use)
    /// </summary>
    internal List<ModuleDescriptor> Modules { get; }
}
```

### StartupBase Class

```
public abstract class StartupBase : IStartup
{
    /// <summary>
    /// Configure module services
    /// </summary>
    public virtual void ConfigureServices(IServiceCollection services);
    
    /// <summary>
    /// Configure module pipeline
    /// </summary>
    public virtual void Configure(
        IApplicationBuilder app, 
        IEndpointRouteBuilder routes
    );
}
```

## 🎯 Best Practices

### 1. Module Naming Conventions

- Module assembly: `{BusinessArea}Module.dll`
- Namespace: `Company.Modules.{BusinessArea}`
- Config file: `module.json`

### 2. Module Isolation Guidelines

Each module should:

- Contain its own business logic
- Avoid depending on concrete implementations of other modules
- Communicate with other modules via interfaces or events
- Have its own configuration file