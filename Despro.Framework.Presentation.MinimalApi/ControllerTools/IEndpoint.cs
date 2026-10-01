using Asp.Versioning.Builder;
using Despro.Framework.Presentation.Modules;
using Microsoft.AspNetCore.Routing;

namespace Despro.Framework.Presentation.MinimalApi.ControllerTools;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app, ApiVersionSet versionSet);
    void MapEndpoint(IEndpointRouteBuilder app, ApiVersionSet versionSet, EndpointModuleContext context) => MapEndpoint(app, versionSet);

    string? Route { get; }
    string? Tag { get; }
    string? GroupName { get; }
    double Version { get; }
}