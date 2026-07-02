using Despro.Framework.Presentation.Middlewares;
using Despro.Framework.Presentation.Utilites;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Hosting;

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
            app.UseSwaggerUI(options =>
            {
                var descriptions = app.DescribeApiVersions();

                foreach (var description in descriptions)
                {
                    var url = $"/swagger/{description.GroupName}/swagger.json";
                    var name = description.GroupName.ToUpperInvariant();
                    options.SwaggerEndpoint(url, name);
                }

                options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
                options.DefaultModelsExpandDepth(-1);
                options.DisplayRequestDuration();
                options.ShowExtensions();

                options.EnableFilter();
                options.ShowCommonExtensions();
                options.EnableDeepLinking();

                options.ConfigObject.PersistAuthorization = true;
                options.EnablePersistAuthorization();
            });

            app.UseDeveloperExceptionPage();
            app.UseHsts();
            app.UseHttpsRedirection();
        }

        if (!app.Environment.IsDevelopment())
        {
            if (ShowSwaggerInProduction)
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    var descriptions = app.DescribeApiVersions();

                    foreach (var description in descriptions)
                    {
                        var url = $"/swagger/{description.GroupName}/swagger.json";
                        var name = description.GroupName.ToUpperInvariant();
                        options.SwaggerEndpoint(url, name);
                    }

                    options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
                    options.DefaultModelsExpandDepth(-1);
                    options.DisplayRequestDuration();
                    options.ShowExtensions();

                    options.EnableFilter();
                    options.ShowCommonExtensions();
                    options.EnableDeepLinking();

                    options.ConfigObject.PersistAuthorization = true;
                    options.EnablePersistAuthorization();
                });
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
}