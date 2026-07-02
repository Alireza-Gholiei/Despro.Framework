using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Despro.Framework.Presentation.ControllerTools;

public class ModelStateUtil
{
    public static string GetModelStateErrors(ModelStateDictionary modelState)
    {
        Dictionary<string, List<string>> errors = new();

        if (modelState is { IsValid: false, ErrorCount: > 0 })
        {
            for (var i = 0; i < modelState.Values.Count(); i++)
            {
                var key = modelState.Keys.ElementAt(i);
                var value = modelState.Values.ElementAt(i);

                if (value.ValidationState == ModelValidationState.Invalid)
                {
                    errors.Add(key, value.Errors.Select(x => string.IsNullOrEmpty(x.ErrorMessage)
                        ? x.Exception?.Message
                            : x.ErrorMessage)
                        .ToList()!);
                }
            }
        }

        var error = string.Join(" - ", errors.Select(x => $"{string.Join(" - ", x.Value)}"));

        return error;
    }
}