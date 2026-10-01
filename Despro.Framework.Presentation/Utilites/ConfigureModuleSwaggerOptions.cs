using System.Reflection;
using Asp.Versioning.ApiExplorer;
using Despro.Framework.Presentation.Modules;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Despro.Framework.Presentation.Utilites;

internal sealed class ConfigureModuleSwaggerOptions(
    IApiVersionDescriptionProvider provider,
    IEnumerable<IModuleRegistration> registrations) : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        var all = registrations.ToList();
        var docs = new Dictionary<string, (string Module, string Group)>();

        foreach (var module in all.SeparateDocModules())
            foreach (var v in provider.ApiVersionDescriptions)
            {
                var name = ModuleSwaggerNames.DocName(module, v.GroupName);
                docs[name] = (module, v.GroupName);
                options.SwaggerDoc(name, new OpenApiInfo { Title = $"{module} API", Version = v.ApiVersion.ToString() });
            }

        if (docs.Count == 0) return;

        var byAssembly = all.GroupBy(r => r.ApiAssembly).ToDictionary(g => g.Key, g => g.First().ModuleName);

        options.DocInclusionPredicate((docName, api) =>
        {
            if (!docs.TryGetValue(docName, out var target))
                return api.GroupName is null || api.GroupName == docName;

            return string.Equals(ResolveModule(api, byAssembly), target.Module, StringComparison.OrdinalIgnoreCase)
                   && api.GroupName == target.Group;
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