using Asp.Versioning;
using Asp.Versioning.Conventions;
using Despro.Framework.Presentation.MinimalApi.ControllerTools;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Despro.Framework.Presentation.MinimalApi;

public static class FrameworkPresentationWebUseApp
{
    public static IApplicationBuilder UseFrameworkPresentationWebMinimalApi(this WebApplication app, IEnumerable<ApiVersion> apiVersions)
    {
        using (var scope = app.Services.CreateScope())
        {
            var endpoints = scope.ServiceProvider.GetServices<IEndpoint>();

            var versionSet = app.NewApiVersionSet()
                .HasApiVersions(apiVersions)
                .ReportApiVersions()
                .Build();

            foreach (var endpoint in endpoints)
            {
                endpoint.MapEndpoint(app, versionSet);
            }
        }

        app.MapControllers();

        return app;
    }
}