using Despro.Framework.Presentation.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace Despro.Framework.Presentation.MinimalApi.Utilites;

public static class EndpointExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddDesproModuleEndpoints(Action<ModuleEndpointOptions> configure)
        {
            var options = new ModuleEndpointOptions();
            configure(options);
            ArgumentException.ThrowIfNullOrWhiteSpace(options.Name);
            ArgumentNullException.ThrowIfNull(options.ApiAssembly);

            services.AddSingleton(new ModuleEndpointRegistration(options));
            return services;
        }

        public IServiceCollection AddFrameworkPresentationWebMinimalApiJson()
        {
            services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(o =>
                o.JsonSerializerOptions.PropertyNamingPolicy = null);
            services.ConfigureHttpJsonOptions(o => o.SerializerOptions.PropertyNamingPolicy = null);

            return services;
        }
    }
}