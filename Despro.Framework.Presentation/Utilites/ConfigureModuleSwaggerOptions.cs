using Asp.Versioning.ApiExplorer;
using Despro.Framework.Presentation.Modules;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Despro.Framework.Presentation.Utilites;

internal sealed class ConfigureModuleSwaggerOptions(
    IApiVersionDescriptionProvider provider,
    IApiDescriptionGroupCollectionProvider apiDescriptionProvider,
    IEnumerable<IModuleRegistration> registrations) : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        var all = registrations.ToList();
        var separateModules = all.SeparateDocModules();
        var byAssembly = all.GroupBy(r => r.ApiAssembly)
            .ToDictionary(g => g.Key, g => g.First().ModuleName);
        var moduleGroups = apiDescriptionProvider.ApiDescriptionGroups.Items
            .SelectMany(group => group.Items)
            .Select(api => (Api: api, Module: ResolveModule(api, byAssembly)))
            .Where(item => item.Module is not null && separateModules.Contains(item.Module, StringComparer.OrdinalIgnoreCase))
            .GroupBy(item => item.Module!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.Select(item => item.Api.GroupName).OfType<string>().ToHashSet(StringComparer.Ordinal),
                StringComparer.OrdinalIgnoreCase);
        var docs = new Dictionary<string, (string Module, string Group)>();

        foreach (var module in separateModules)
            if (moduleGroups.TryGetValue(module, out var groups))
                foreach (var v in provider.ApiVersionDescriptions.Where(v => groups.Contains(v.GroupName)))
            {
                var name = ModuleSwaggerNames.DocName(module, v.GroupName);
                docs[name] = (module, v.GroupName);
                options.SwaggerDoc(name, new OpenApiInfo { Title = $"{module} API", Version = v.ApiVersion.ToString() });
            }

        if (docs.Count == 0) return;

        options.DocInclusionPredicate((docName, api) =>
        {
            var module = ResolveModule(api, byAssembly);

            if (docs.TryGetValue(docName, out var target))
                return string.Equals(module, target.Module, StringComparison.OrdinalIgnoreCase) && api.GroupName == target.Group;

            if (api.GroupName is not null && api.GroupName != docName)
                return false;

            return module is null
                   || !separateModules.Contains(module, StringComparer.OrdinalIgnoreCase);

        });
    }

    private static string? ResolveModule(ApiDescription api, Dictionary<Assembly, string> byAssembly)
    {
        var fromMetadata = api.ActionDescriptor.EndpointMetadata
            .OfType<EndpointModuleMetadata>().FirstOrDefault()?.ModuleName;
        if (fromMetadata is not null) return fromMetadata;

        return api.ActionDescriptor is ControllerActionDescriptor c
               && byAssembly.TryGetValue(c.ControllerTypeInfo.Assembly, out var module)
            ? module
            : null;
    }
}