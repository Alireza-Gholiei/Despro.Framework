using Microsoft.AspNetCore.Builder;

namespace Despro.Framework.Presentation.Modules;

public static class ControllerMappingExtensions
{
    private const string Key = "Despro.Framework.ControllersMapped";

    public static void MapControllersOnce(this WebApplication app)
    {
        var props = ((IApplicationBuilder)app).Properties;
        if (props.ContainsKey(Key)) return;
        props[Key] = true;
        app.MapControllers();
    }
}