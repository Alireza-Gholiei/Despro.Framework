using Asp.Versioning;
using System.Reflection;

namespace Despro.Framework.Presentation.Modules;

public sealed class ModuleEndpointOptions
{
    public string Name { get; set; } = default!;
    public Assembly ApiAssembly { get; set; } = default!;
    public string RoutePrefix { get; set; } = "api/[module]/v{version:apiVersion}/[controller]";
    public IReadOnlyList<ApiVersion>? Versions { get; set; }
    public bool SeparateSwaggerDoc { get; set; } = true;
}

public sealed record EndpointModuleMetadata(string ModuleName);

public sealed record EndpointModuleContext(string ModuleName, string RoutePrefix)
{
    public static readonly EndpointModuleContext Default = new("default", string.Empty);

    public string ResolveModuleToken(string template) =>
        template.Replace("[module]", ModuleName.ToLowerInvariant());

    public string Resolve(string tag) => ResolveModuleToken(RoutePrefix).Replace("[controller]", tag);
}

public static class ModuleSwaggerNames
{
    public static string DocName(string module, string versionGroup) => $"{module.ToLowerInvariant()}-{versionGroup}";
}