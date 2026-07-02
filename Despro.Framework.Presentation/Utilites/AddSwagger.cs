using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Despro.Framework.Presentation.Utilites;

public static class SwaggerConfig
{
    public static IServiceCollection AddSwagger(this IServiceCollection services, string applicationName)
    {
        services.AddEndpointsApiExplorer();

        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        services.AddSingleton<IConfigureOptions<SwaggerGenOptions>>(sp =>
        {
            var provider = sp.GetRequiredService<IApiVersionDescriptionProvider>();

            return new ConfigureSwaggerOptions(provider, applicationName);
        });

        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please Insert JWT With Bearer Into Field",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
            });
            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });

            //options.AddSecurityRequirement(new OpenApiSecurityRequirement {
            //{
            //    new OpenApiSecurityScheme
            //    {
            //        Reference = new OpenApiReference
            //        {
            //            Type = ReferenceType.SecurityScheme,
            //            Id = "Bearer"
            //        }
            //    }, []
            //}});

            options.AddSecurityDefinition("RoleId", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please Insert RoleId Into Field",
                Name = "RoleId",
                Type = SecuritySchemeType.ApiKey
            });
            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("RoleId", document)] = []
            });

            //options.AddSecurityRequirement(new OpenApiSecurityRequirement {{
            //    new OpenApiSecurityScheme
            //    {
            //        Reference = new OpenApiReference
            //        {
            //            Type = ReferenceType.SecurityScheme,
            //            Id = "RoleId"
            //        }
            //    },
            //    []
            //}});
        });

        return services;
    }
}