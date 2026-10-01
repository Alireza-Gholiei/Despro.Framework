using System.Reflection;

namespace Despro.Framework.Presentation.Modules;

public interface IModuleRegistration
{
    string ModuleName { get; }
    Assembly ApiAssembly { get; }
    bool SeparateSwaggerDoc { get; }
}

public sealed record ModuleEndpointRegistration(ModuleEndpointOptions Options) : IModuleRegistration
{
    public string ModuleName => Options.Name;
    public Assembly ApiAssembly => Options.ApiAssembly;
    public bool SeparateSwaggerDoc => Options.SeparateSwaggerDoc;
}

public static class ModuleRegistrationExtensions
{
    public static IReadOnlyList<string> SeparateDocModules(this IEnumerable<IModuleRegistration> regs) =>
        regs.GroupBy(r => r.ModuleName, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Any(r => r.SeparateSwaggerDoc))
            .Select(g => g.Key)
            .ToList();

    public static void EnsureValid(this IEnumerable<IModuleRegistration> regs)
    {
        var clash = regs.GroupBy(r => r.ApiAssembly)
            .FirstOrDefault(g => g.Select(r => r.ModuleName).Distinct(StringComparer.OrdinalIgnoreCase).Count() > 1);

        if (clash is not null)
            throw new InvalidOperationException($"Assembly '{clash.Key.GetName().Name}' is registered for more than one module: " +
                                                $"{string.Join(", ", clash.Select(r => r.ModuleName).Distinct())}.");
    }
}