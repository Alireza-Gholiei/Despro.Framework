using Despro.Framework.Presentation.MinimalApi.Utilites;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Despro.Framework.Presentation.MinimalApi;

public static class FrameworkPresentationWebDi
{
    /// <summary>
    /// AddFrameworkPresentationWebApi
    /// </summary>
    /// <param name="RoutePrefix">v{version:apiVersion}/[controller]</param>
    /// <param name="services"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static IServiceCollection AddFrameworkPresentationWebMinimalApi(this IServiceCollection services, Assembly ApiAssembly, string RoutePrefix)
    {
        services.AddDesproModuleEndpoints(o =>
        {
            o.Name = "default";
            o.ApiAssembly = ApiAssembly;
            o.RoutePrefix = RoutePrefix;
            o.SeparateSwaggerDoc = false;
        });

        return services.AddFrameworkPresentationWebMinimalApiJson();
    }
}