using Despro.Framework.Base;
using Despro.Framework.WebClient.IRepository;
using Despro.Framework.WebClient.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace Despro.Framework.WebClient;

public static class FrameworkWebClientDi
{
    public static IServiceCollection AddFrameworkWebClient(this IServiceCollection services)
    {
        services.AddScoped<IHttp, Http>();

        services.AddHttpClient(Constants.HttpClientName, client =>
        {
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromMinutes(5);
        }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ClientCertificateOptions = ClientCertificateOption.Manual,
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        });

        return services;
    }
}