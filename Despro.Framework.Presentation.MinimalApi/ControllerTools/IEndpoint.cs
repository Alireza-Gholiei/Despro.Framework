using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Routing;

namespace Despro.Framework.Presentation.MinimalApi.ControllerTools;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app, ApiVersionSet versionSet);
    string? Route { get; }
    string? Tag { get; }
    string? GroupName { get; }
    double Version { get; }
}