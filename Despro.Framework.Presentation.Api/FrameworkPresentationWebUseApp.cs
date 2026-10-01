using Despro.Framework.Presentation.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Despro.Framework.Presentation.Api;

public static class FrameworkPresentationWebUseApp
{
    public static IApplicationBuilder UseFrameworkPresentationWebApi(this WebApplication app)
    {
        app.Services.GetServices<IModuleRegistration>().EnsureValid();

        var duplicate = app.Services.GetServices<ModuleControllerRegistration>()
            .GroupBy(r => r.ModuleName, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(g => g.Count() > 1);
        if (duplicate is not null)
            throw new InvalidOperationException($"Module '{duplicate.Key}' registered controllers more than once.");

        app.MapControllersOnce();
        return app;
    }
}