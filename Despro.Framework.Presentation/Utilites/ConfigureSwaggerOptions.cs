using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Despro.Framework.Presentation.Utilites;

public class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider, string ApplicationName)
    : IConfigureNamedOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        foreach (var item in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(item.GroupName, CreateVersionInfo(item));
        }
    }

    public void Configure(string name, SwaggerGenOptions options)
    {
        Configure(options);
    }

    private OpenApiInfo CreateVersionInfo(ApiVersionDescription description)
    {
        OpenApiInfo info = new()
        {
            Title = ApplicationName,
            Version = description.ApiVersion.ToString()
        };

        if (description.IsDeprecated)
        {
            info.Description += "This API Version Has Been Deprecated.";
        }

        return info;
    }
}