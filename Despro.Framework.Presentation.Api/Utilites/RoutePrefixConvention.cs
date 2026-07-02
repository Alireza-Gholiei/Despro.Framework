using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Despro.Framework.Presentation.Api.Utilites;

public class RoutePrefixConvention(string routePrefix) : IApplicationModelConvention
{
    private readonly AttributeRouteModel _routePrefix = new(new RouteAttribute(routePrefix));

    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        {
            if (!controller.Attributes.Any(a => a is ApiControllerAttribute))
                continue;

            foreach (var selector in controller.Selectors)
            {
                selector.AttributeRouteModel ??= _routePrefix;
            }
        }
    }
}