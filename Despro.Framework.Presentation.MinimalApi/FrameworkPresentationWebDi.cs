using Despro.Framework.Presentation.MinimalApi.Utilites;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Despro.Framework.Presentation.MinimalApi;

public static class FrameworkPresentationWebDi
{
    public static string _routePrefix = string.Empty;
    /// <summary>
    /// AddFrameworkPresentationWebApi
    /// </summary>
    /// <param name="services"></param>
    /// <param name="RoutePrefix">v{version:apiVersion}/[controller]</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static IServiceCollection AddFrameworkPresentationWebMinimalApi(this IServiceCollection services,
        Assembly ApiAssembly,
        string RoutePrefix)
    {
        _routePrefix = RoutePrefix;

        services.AddAllEndpoints(ApiAssembly);

        services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = null;
        });

        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = null;
        });

        return services;
    }
}