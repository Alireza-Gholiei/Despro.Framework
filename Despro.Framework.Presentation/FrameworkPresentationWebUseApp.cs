using Despro.Framework.Presentation.Middlewares;
using Despro.Framework.Presentation.Modules;
using Despro.Framework.Presentation.Utilites;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Despro.Framework.Presentation;

public static class FrameworkPresentationWebUseApp
{
    public static IApplicationBuilder UseFrameworkPresentationWeb(this WebApplication app, bool ShowSwaggerInProduction)
    {
        DatePersian.InitializePersianCulture();
        app.UseRequestLocalization();

        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
            ForwardLimit = 10
        });

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(o => ConfigureSwaggerUi(o, app));
            app.UseDeveloperExceptionPage();
            app.UseHsts();
            app.UseHttpsRedirection();
        }

        if (!app.Environment.IsDevelopment())
        {
            if (ShowSwaggerInProduction)
            {
                app.UseSwagger();
                app.UseSwaggerUI(o => ConfigureSwaggerUi(o, app));
            }
            app.UseHsts();
            app.UseHttpsRedirection();
        }

        app.UseRouting();
        app.UseCors(FrameworkPresentationWebDi._corsPolicyName);

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.UseApiExceptionHandler();

        return app;
    }

    private static void ConfigureSwaggerUi(SwaggerUIOptions options, WebApplication app)
    {
        var versions = app.DescribeApiVersions();

        foreach (var module in app.Services.GetServices<IModuleRegistration>().SeparateDocModules())
            foreach (var d in versions)
                options.SwaggerEndpoint(
                    $"/swagger/{ModuleSwaggerNames.DocName(module, d.GroupName)}/swagger.json",
                    $"{module} {d.GroupName.ToUpperInvariant()}");

        options.DocExpansion(DocExpansion.None);
        options.DefaultModelsExpandDepth(-1);
        options.DisplayRequestDuration();
        options.ShowExtensions();
        options.EnableFilter();
        options.ShowCommonExtensions();
        options.EnableDeepLinking();
        options.ConfigObject.PersistAuthorization = true;
        options.EnablePersistAuthorization();
    }
}