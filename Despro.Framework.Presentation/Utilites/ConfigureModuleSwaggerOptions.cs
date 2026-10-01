using Asp.Versioning.ApiExplorer;
using Despro.Framework.Presentation.Modules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Despro.Framework.Presentation.Utilites;

internal sealed class ConfigureModuleSwaggerOptions(
    IApiVersionDescriptionProvider provider,
    IEnumerable<ModuleEndpointRegistration> registrations) : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        var docs = new Dictionary<string, (string Module, string Group)>();

        foreach (var m in registrations.Select(r => r.Options).Where(o => o.SeparateSwaggerDoc))
            foreach (var v in provider.ApiVersionDescriptions)
            {
                var name = ModuleSwaggerNames.DocName(m.Name, v.GroupName);
                docs[name] = (m.Name, v.GroupName);
                options.SwaggerDoc(name, new OpenApiInfo { Title = $"{m.Name} API", Version = v.ApiVersion.ToString() });
            }

        if (docs.Count == 0) return;

        options.DocInclusionPredicate((docName, api) =>
        {
            if (!docs.TryGetValue(docName, out var target))
                return api.GroupName is null || api.GroupName == docName;

            var module = api.ActionDescriptor.EndpointMetadata
                .OfType<EndpointModuleMetadata>().FirstOrDefault()?.ModuleName;

            return string.Equals(module, target.Module, StringComparison.OrdinalIgnoreCase) && api.GroupName == target.Group;
        });
    }
}