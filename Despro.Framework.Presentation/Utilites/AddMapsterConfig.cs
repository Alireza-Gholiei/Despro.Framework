using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Despro.Framework.Presentation.Utilites;

public static class MapsterConfig
{
    public static IServiceCollection AddMapsterConfig(this IServiceCollection services)
    {
        TypeAdapterConfig.GlobalSettings
            .ForType<DateTime, long>()
            .MapWith(src => src.Ticks);
        TypeAdapterConfig.GlobalSettings
            .ForType<DateTime?, long>()
            .MapWith(src => src.HasValue ? src.Value.Ticks : 0);

        TypeAdapterConfig.GlobalSettings
            .ForType<long, DateTime>()
            .MapWith(src => new DateTime(src));
        TypeAdapterConfig.GlobalSettings
            .ForType<long?, DateTime>()
            .MapWith(src => src.HasValue ? new DateTime(src.Value) : new DateTime());

        var typeAdapterConfig = TypeAdapterConfig.GlobalSettings;

        typeAdapterConfig.Scan(Assembly.GetExecutingAssembly());

        var mapperConfig = new Mapper(typeAdapterConfig);

        services.AddSingleton<IMapper>(mapperConfig);

        return services;
    }
}