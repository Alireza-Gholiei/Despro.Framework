using Despro.Framework.Presentation.Api.Utilites;
using Despro.Framework.Presentation.ControllerTools;
using Microsoft.Extensions.DependencyInjection;
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
        services.AddControllers(option =>
            {
                option.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
                option.Conventions.Add(new RoutePrefixConvention(RoutePrefix));
            })
            .AddNewtonsoftJson(option => option.SerializerSettings.ContractResolver = new DefaultContractResolver())
            .AddJsonOptions(option => option.JsonSerializerOptions.PropertyNamingPolicy = null)
            .ConfigureApiBehaviorOptions(option =>
            {
                option.SuppressModelStateInvalidFilter = true;
                option.InvalidModelStateResponseFactory = context =>
                    throw new Exception(ModelStateUtilites.GetModelStateErrors(context.ModelState));
            });

        return services;
    }
}