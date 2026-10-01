using Asp.Versioning;
using Asp.Versioning.Conventions;
using Despro.Framework.Presentation.MinimalApi.ControllerTools;
using Despro.Framework.Presentation.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Despro.Framework.Presentation.MinimalApi;

public static class FrameworkPresentationWebUseApp
{
    public static IApplicationBuilder UseFrameworkPresentationWebMinimalApi(this WebApplication app, IEnumerable<ApiVersion> apiVersions)
    {
        var modules = app.Services.GetServices<ModuleEndpointRegistration>().Select(r => r.Options).ToList();

        var duplicate = modules.GroupBy(m => m.Name, StringComparer.OrdinalIgnoreCase).FirstOrDefault(g => g.Count() > 1);
        if (duplicate is not null)
            throw new InvalidOperationException($"Module '{duplicate.Key}' registered endpoints more than once.");

        using var scope = app.Services.CreateScope();

        foreach (var module in modules)
        {
            var versions = module.Versions ?? apiVersions?.ToList() ?? [new ApiVersion(1, 0)];

            var versionSet = app.NewApiVersionSet(module.Name)
                .HasApiVersions(versions)
                .ReportApiVersions()
                .Build();

            var context = new EndpointModuleContext(module.Name, module.RoutePrefix);

            var endpointTypes = module.ApiAssembly.GetTypes()
                .Where(t => t is { IsInterface: false, IsAbstract: false } && t.IsAssignableTo(typeof(IEndpoint)));

            foreach (var type in endpointTypes)
            {
                var endpoint = (IEndpoint)ActivatorUtilities.CreateInstance(scope.ServiceProvider, type);
                endpoint.MapEndpoint(app, versionSet, context);
            }
        }

        app.MapControllers();
        return app;
    }
}