using Microsoft.AspNetCore.Builder;

namespace Despro.Framework.Presentation.Api;

public static class FrameworkPresentationWebUseApp
{
    public static IApplicationBuilder UseFrameworkPresentationWebApi(this WebApplication app)
    {
        app.MapControllers();

        return app;
    }
}