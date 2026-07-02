using Despro.Framework.Presentation.MinimalApi.ControllerTools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace Despro.Framework.Presentation.MinimalApi.Utilites;

public static class EndpointExtensions
{
    public static void AddAllEndpoints(this IServiceCollection services, Assembly assembly)
    {
        var serviceDescriptors = assembly.GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IEndpoint)) && t is { IsInterface: false, IsAbstract: false })
            .Select(type => ServiceDescriptor.Scoped(typeof(IEndpoint), type));

        services.TryAddEnumerable(serviceDescriptors);
    }
}