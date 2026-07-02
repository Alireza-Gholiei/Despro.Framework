using Despro.Framework.Domain.ValueObjects.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Despro.Framework.Domain;

public static class FrameworkDomainDi
{
    public static IServiceCollection AddFrameworkDomain(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<AuthPasswordOptions>()
            .Bind(configuration.GetSection(AuthPasswordOptions.ConfigName))
            .ValidateDataAnnotations()
            .Validate(options => options.RequiredUniqueChars <= options.RequiredLength, "تعداد نویسه‌های متمایز نباید بیشتر از طول رمز عبور باشد.")
            .ValidateOnStart();

        return services;
    }
}