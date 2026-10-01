using Despro.Framework.Presentation.Modules;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Despro.Framework.Presentation.Api.Utilites;

internal sealed class ConfigureControllerRoutes(IEnumerable<ModuleControllerRegistration> modules,
    IEnumerable<DefaultControllerRoute> defaults) : IConfigureOptions<MvcOptions>
{
    public void Configure(MvcOptions options) =>
        options.Conventions.Add(new RoutePrefixConvention(
            defaults.LastOrDefault()?.RoutePrefix,
            modules.Select(m => m.Options).ToList()));
}