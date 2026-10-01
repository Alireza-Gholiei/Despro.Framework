using Despro.Framework.Presentation.Modules;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Despro.Framework.Presentation.Api.Utilites;

public class RoutePrefixConvention(string? routePrefix, IReadOnlyList<ModuleControllerOptions>? modules = null)
    : IApplicationModelConvention
{
    private readonly AttributeRouteModel? _default = string.IsNullOrWhiteSpace(routePrefix) ? null : new AttributeRouteModel(new RouteAttribute(routePrefix));
    private readonly IReadOnlyList<ModuleControllerOptions> _modules = modules ?? [];

    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        {
            if (!controller.Attributes.Any(a => a is ApiControllerAttribute))
                continue;

            var module = _modules.FirstOrDefault(m => m.ControllerAssembly == controller.ControllerType.Assembly);

            var prefix = module is null
                ? _default
                : new AttributeRouteModel(new RouteAttribute(
                    module.RoutePrefix.Replace("[module]", module.Name.ToLowerInvariant())));

            if (prefix is null) continue;

            foreach (var selector in controller.Selectors)
                selector.AttributeRouteModel ??= prefix;
        }
    }
}