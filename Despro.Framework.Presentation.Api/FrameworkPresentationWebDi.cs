using Despro.Framework.Presentation.Api.Utilites;
using Despro.Framework.Presentation.ControllerTools;
using Despro.Framework.Presentation.Modules;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Serialization;

namespace Despro.Framework.Presentation.Api;

public static class FrameworkPresentationWebDi
{
    /// <summary>
    /// AddFrameworkPresentationWebApi
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="RoutePrefix">v{version:apiVersion}/[controller]</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static IServiceCollection AddFrameworkPresentationWebApi(this IServiceCollection services, string RoutePrefix)
    {
        services.AddSingleton(new DefaultControllerRoute(RoutePrefix));
        return services.AddFrameworkPresentationWebApiCore();
    }

    public static IServiceCollection AddFrameworkPresentationWebApi(this IServiceCollection services)
        => services.AddFrameworkPresentationWebApiCore();

    public static IServiceCollection AddDesproModuleControllers(this IServiceCollection services, Action<ModuleControllerOptions> configure)
    {
        var options = new ModuleControllerOptions();
        configure(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.Name);
        ArgumentNullException.ThrowIfNull(options.ControllerAssembly);

        var registration = new ModuleControllerRegistration(options);
        services.AddSingleton(registration);
        services.AddSingleton<IModuleRegistration>(registration);

        services.AddControllers().ConfigureApplicationPartManager(manager =>
        {
            if (manager.ApplicationParts.OfType<AssemblyPart>().All(p => p.Assembly != options.ControllerAssembly))
                manager.ApplicationParts.Add(new AssemblyPart(options.ControllerAssembly));
        });

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<MvcOptions>, ConfigureControllerRoutes>());
        return services;
    }

    private static IServiceCollection AddFrameworkPresentationWebApiCore(this IServiceCollection services)
    {
        services.AddControllers(option =>
                option.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true)
            .AddNewtonsoftJson(option => option.SerializerSettings.ContractResolver = new DefaultContractResolver())
            .AddJsonOptions(option => option.JsonSerializerOptions.PropertyNamingPolicy = null)
            .ConfigureApiBehaviorOptions(option =>
            {
                option.SuppressModelStateInvalidFilter = true;
                option.InvalidModelStateResponseFactory = context =>
                    throw new Exception(ModelStateUtilites.GetModelStateErrors(context.ModelState));
            });

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<MvcOptions>, ConfigureControllerRoutes>());
        return services;
    }
}