using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Despro.Framework.Presentation.Modules;

public sealed class ModuleControllerOptions
{
    public string Name { get; set; } = default!;                    
    public Assembly ControllerAssembly { get; set; } = default!;
    public string RoutePrefix { get; set; } = "api/[module]/v{version:apiVersion}/[controller]";
    public bool SeparateSwaggerDoc { get; set; } = true;
}

public sealed record ModuleControllerRegistration(ModuleControllerOptions Options) : IModuleRegistration
{
    public string ModuleName => Options.Name;
    public Assembly ApiAssembly => Options.ControllerAssembly;
    public bool SeparateSwaggerDoc => Options.SeparateSwaggerDoc;
}

public sealed record DefaultControllerRoute(string RoutePrefix);