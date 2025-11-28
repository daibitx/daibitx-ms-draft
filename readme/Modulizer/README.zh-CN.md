# Daibitx.Extension.Modularize

[English](./README.md)|中文

[![NuGet](https://img.shields.io/nuget/v/Daibitx.Extension.Modularize.svg)](https://www.nuget.org/packages/Daibitx.Extension.Modularize)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Daibitx.Extension.Modularize 是一个为ASP.NET Core设计的模块化应用框架，提供插件化架构支持，让您的应用能够动态加载和管理独立模块。

## ✨ 功能特性

- 🔌 **动态模块发现** - 自动扫描并加载指定目录中的模块
- 📦 **模块隔离** - 每个模块运行在独立的Assembly上下文中
- 🔄 **依赖注入集成** - 完美集成ASP.NET Core DI容器
- ⚙️ **配置管理** - 支持模块级配置文件
- 🚀 **生命周期管理** - 完整的模块加载、初始化、配置流程
- 🔧 **灵活配置** - 可自定义模块路径和加载行为

## 📦 安装

```bash
dotnet add package Daibitx.Extension.Modularize
```

## 🚀 快速开始

### 1. 基本配置

```csharp
// Program.cs
using Daibitx.Extension.Modularize;

var builder = WebApplication.CreateBuilder(args);

// 配置模块化服务
builder.ConfigureModulesService(options =>
{
    // 设置模块存放路径
    options.ModulesPath = Path.Combine(AppContext.BaseDirectory, "Modules");
});

var app = builder.Build();

// 配置模块管道
app.ConfigureModules(app, app, app.Services);

app.Run();
```

### 2. 创建模块

#### 2.1 创建模块启动类

```csharp
// MyModule/Startup.cs
using Daibitx.Extension.Modularize.Abstractons;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

public class MyModuleStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        // 注册模块服务
        services.AddScoped<IMyModuleService, MyModuleService>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes)
    {
        // 配置模块路由
        routes.MapControllerRoute(
            name: "mymodule",
            pattern: "mymodule/{controller=Home}/{action=Index}/{id?}");
    }
}
```

#### 2.2 创建模块配置文件

```json
// MyModule/MyModule.json
{
  "AssemblyName": "MyModule",
  "DllPath": "MyModule.dll",
  "ConfigPath": "MyModule.json",
  "AssemblyContextPath": "MyModule"
}
```

### 3. 模块目录结构

```
YourApp/
├── Modules/
│   ├── MyModule/
│   │   ├── MyModule.dll          # 模块程序集
│   │   ├── MyModule.json           # 模块配置文件
│   └── AnotherModule/
│       ├── AnotherModule.dll
│       └── AnotherModule.json
├── YourApp.dll
└── appsettings.json
```

## 📋 API 参考

### HostServiceExtension 扩展方法

#### ConfigureModulesService

配置模块化服务。

```csharp
public static void ConfigureModulesService(
    this WebApplicationBuilder application, 
    Action<ModuleOptions> moduleOptions
)
```

**参数：**
- `moduleOptions` - 模块配置委托

**示例：**
```csharp
builder.ConfigureModulesService(options =>
{
    options.ModulesPath = "./Modules";
});
```

#### ConfigureModules

配置模块管道。

```csharp
public static void ConfigureModules(
    this IApplicationBuilder builder, 
    IEndpointRouteBuilder routes, 
    IServiceProvider serviceProvider
)
```

**参数：**
- `builder` - 应用程序构建器
- `routes` - 路由构建器
- `serviceProvider` - 服务提供程序

### ModuleOptions 配置类

```csharp
public class ModuleOptions
{
    /// <summary>
    /// 模块存放路径
    /// </summary>
    public string ModulesPath { get; set; }
    
    /// <summary>
    /// 已加载的模块列表（内部使用）
    /// </summary>
    internal List<ModuleDescriptor> Modules { get; }
}
```

### StartupBase 基类

```csharp
public abstract class StartupBase : IStartup
{
    /// <summary>
    /// 配置模块服务
    /// </summary>
    public virtual void ConfigureServices(IServiceCollection services);
    
    /// <summary>
    /// 配置模块管道
    /// </summary>
    public virtual void Configure(
        IApplicationBuilder app, 
        IEndpointRouteBuilder routes
    );
}
```

## 🎯 最佳实践

### 1. 模块命名规范

- 模块程序集名称：`{BusinessArea}Module.dll`
- 命名空间：`Company.Modules.{BusinessArea}`
- 配置文件：`module.json`

### 2. 模块隔离原则

每个模块应该：
- 拥有独立的业务逻辑
- 不依赖其他模块的具体实现
- 通过接口或事件进行模块间通信
- 包含自己的配置文件
