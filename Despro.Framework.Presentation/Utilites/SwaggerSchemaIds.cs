namespace Despro.Framework.Presentation.Utilites;

public static class SwaggerSchemaIds
{
    public static string Create(Type type)
    {
        if (type.IsArray) return $"{Create(type.GetElementType()!)}Array";
        if (!type.IsGenericType) return (type.FullName ?? type.Name).Replace('+', '.');

        var definition = type.GetGenericTypeDefinition().FullName!;
        var baseName = definition[..definition.IndexOf('`')].Replace('+', '.');
        return $"{baseName}Of{string.Join("And", type.GetGenericArguments().Select(Create))}";
    }
}