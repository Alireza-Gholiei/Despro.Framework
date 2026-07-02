using Despro.Framework.Presentation.Utilites;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace Despro.Framework.Presentation;

public static class FrameworkPresentationWebDi
{
    public static string _corsPolicyName = string.Empty;
    /// <summary>
    /// AddFrameworkPresentationWeb
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="RoutePrefix">v{version:apiVersion}/[controller]</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static IServiceCollection AddFrameworkPresentationWeb(this IServiceCollection services,
        IConfiguration configuration,
        string ApplicationName,
        string CorsPolicyName)
    {
        services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Arabic));

        _corsPolicyName = CorsPolicyName;

        services.AddJwtAuthentication(configuration)
            .AddSwagger(ApplicationName)
            .AddMapsterConfig();

        services.AddCors(options =>
        {
            options.AddPolicy(name: CorsPolicyName, builder =>
            {
                var origins = configuration["App:CorsOrigins"]?
                    .Split(",", StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToArray();

                if (origins == null || origins.Length == 0)
                    throw new Exception("CorsOrigins is not configured.");

                builder.WithOrigins(origins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        services.AddMemoryCache();

        services.AddHsts(options =>
        {
            options.Preload = true;
            options.IncludeSubDomains = true;
            options.MaxAge = TimeSpan.FromDays(60);
        });

        return services;
    }
}