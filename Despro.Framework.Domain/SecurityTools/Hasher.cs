using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Despro.Framework.Domain.SecurityTools;

public static class Hasher
{
    public static string HashPassword(string username, string password)
    {
        using var sha256Hash = SHA256.Create();

        var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(username + password));

        StringBuilder builder = new();

        foreach (var t in bytes)
        {
            builder.Append(t.ToString("x2"));
        }

        return builder.ToString();
    }

    public static string GetWebConfigPassword()
    {
        var text = "hwe!!!g#%$%6^^fhsdhgedsfasfcshfgs$$$###@!@3kjwhdwhjd6125321";

        text += DateTime.Now.Year;
        text += DateTime.Now.Month;
        text += DateTime.Now.ToShortDateString();
        text += DateTime.Now.Hour;
        text += DateTime.Now.Minute;

        return GetHash(text);
    }

    public static string GetHash(string text)
    {
        using var sha256Hash = SHA256.Create();

        var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(text));

        StringBuilder builder = new();

        foreach (var t in bytes)
        {
            builder.Append(t.ToString("x2"));
        }

        return builder.ToString();
    }

    public static string GetHash(object T)
    {
        var jToken = JToken.Parse("{}");

        foreach (var propertyInfo in T.GetType().GetProperties())
        {
            if (propertyInfo.GetGetMethod() != null && propertyInfo.GetGetMethod()!.IsVirtual)
            {
            }
            else if (propertyInfo.Name != "ID" && propertyInfo.Name != "Id" &&
                     !string.IsNullOrEmpty(propertyInfo.GetValue(T)?.ToString()))
            {
                jToken[propertyInfo.Name] = propertyInfo.GetValue(T)?.ToString();
            }
        }

        var json = JsonConvert.SerializeObject(jToken);

        json += "JKAFHADJKSBFUISEF@!@#!@#!@";
        json = GetHash(json);

        return json;
    }
}