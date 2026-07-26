using Despro.Framework.Base;
using Despro.Framework.Base.BaseModels.HttpModels;
using Despro.Framework.Base.IBaseServices;
using Despro.Framework.WebClient.IRepository;
using Despro.Framework.WebClient.WebClientExceptions;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Reflection;
using System.Text;

namespace Despro.Framework.WebClient.Repository;

public class Http(IAuthService authService, IHttpClientFactory httpClientFactory) : IHttp
{
    public async Task<TOut> GetAsync<TOut>(string url, bool apiResult = true, bool isAuth = true, string token = "", bool isRole = true)
    {
        Uri requestUri = new(url);
        HttpRequestMessage requestMessage = new()
        {
            Method = HttpMethod.Get,
            RequestUri = requestUri
        };

        return await SendUriAsync<TOut>(requestMessage, apiResult, isAuth, token, isRole);
    }

    public async Task<TOut> PostAsync<TIn, TOut>(string url, TIn model, bool apiResult = true, bool isAuth = true, string token = "", bool isRole = true)
    {
        Uri requestUri = new(url);
        var payload = JsonConvert.SerializeObject(model);
        StringContent httpContent = new(payload, Encoding.UTF8, MediaTypeNames.Application.Json);
        HttpRequestMessage requestMessage = new()
        {
            Method = HttpMethod.Post,
            RequestUri = requestUri,
            Content = httpContent
        };
        return await SendUriAsync<TOut>(requestMessage, apiResult, isAuth, token, isRole);
    }

    public async Task<TOut> PostFormAsync<TIn, TOut>(string url, TIn model, bool apiResult = true, bool isAuth = true, string token = "", bool isRole = true)
    {
        using var content = new MultipartFormDataContent();

        if (model != null)
        {
            var localCache = new Dictionary<Type, PropertyInfo[]>();

            var props = typeof(TIn).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in props)
            {
                var value = prop.GetValue(model);
                if (value == null) continue;

                AddToContent(content, value, prop.Name, localCache);
            }
        }

        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
        requestMessage.Content = content;

        return await SendUriAsync<TOut>(requestMessage, apiResult, isAuth, token, isRole);
    }

    public async Task<TOut> PutAsync<TIn, TOut>(string url, TIn model, bool apiResult = true, bool isAuth = true, string token = "", bool isRole = true)
    {
        Uri requestUri = new(url);
        var payload = JsonConvert.SerializeObject(model);
        StringContent httpContent = new(payload, Encoding.UTF8, MediaTypeNames.Application.Json);
        HttpRequestMessage requestMessage = new()
        {
            Method = HttpMethod.Put,
            RequestUri = requestUri,
            Content = httpContent
        };
        return await SendUriAsync<TOut>(requestMessage, apiResult, isAuth, token, isRole);
    }

    public async Task<TOut> PutFormAsync<TIn, TOut>(string url, TIn model, bool apiResult = true, bool isAuth = true, string token = "", bool isRole = true)
    {
        using var content = new MultipartFormDataContent();

        if (model != null)
        {
            var localCache = new Dictionary<Type, PropertyInfo[]>();

            var props = typeof(TIn).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in props)
            {
                var value = prop.GetValue(model);
                if (value == null) continue;

                AddToContent(content, value, prop.Name, localCache);
            }
        }

        using var requestMessage = new HttpRequestMessage(HttpMethod.Put, url);
        requestMessage.Content = content;

        return await SendUriAsync<TOut>(requestMessage, apiResult, isAuth, token, isRole);
    }

    public async Task<TOut> PatchAsync<TIn, TOut>(string url, TIn model, bool apiResult = true, bool isAuth = true, string token = "", bool isRole = true)
    {
        Uri requestUri = new(url);
        var payload = JsonConvert.SerializeObject(model);
        StringContent httpContent = new(payload, Encoding.UTF8, MediaTypeNames.Application.Json);
        HttpRequestMessage requestMessage = new()
        {
            Method = HttpMethod.Patch,
            RequestUri = requestUri,
            Content = httpContent
        };
        return await SendUriAsync<TOut>(requestMessage, apiResult, isAuth, token, isRole);
    }

    public async Task<TOut> PatchFormAsync<TIn, TOut>(string url, TIn model, bool apiResult = true, bool isAuth = true, string token = "", bool isRole = true)
    {
        using var content = new MultipartFormDataContent();

        if (model != null)
        {
            var localCache = new Dictionary<Type, PropertyInfo[]>();

            var props = typeof(TIn).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in props)
            {
                var value = prop.GetValue(model);
                if (value == null) continue;

                AddToContent(content, value, prop.Name, localCache);
            }
        }

        using var requestMessage = new HttpRequestMessage(HttpMethod.Patch, url);
        requestMessage.Content = content;

        return await SendUriAsync<TOut>(requestMessage, apiResult, isAuth, token, isRole);
    }

    public async Task<TOut> DeleteAsync<TOut>(string url, bool apiResult = true, bool isAuth = true, string token = "", bool isRole = true)
    {
        Uri requestUri = new(url);
        HttpRequestMessage requestMessage = new()
        {
            Method = HttpMethod.Delete,
            RequestUri = requestUri
        };

        return await SendUriAsync<TOut>(requestMessage, apiResult, isAuth, token, isRole);
    }

    public async Task<MemoryStream> GetFileAsync(string url, bool apiResult = true, bool isAuth = true, string token = "", bool isRole = true)
    {
        Uri requestUri = new(url);
        HttpRequestMessage requestMessage = new()
        {
            Method = HttpMethod.Get,
            RequestUri = requestUri
        };

        return await SendUriStreamAsync(requestMessage, apiResult, isAuth, token, isRole);
    }

    public async Task<TOut> UploadFile<TOut>(string url, IFormFile file, bool apiResult = true, bool isAuth = true, string token = "", bool isRole = true)
    {
        MultipartFormDataContent httpContent = new();

        StreamContent fileStreamContent = new(file.OpenReadStream());

        httpContent.Add(fileStreamContent, Path.GetFileName(file.Name), file.Name);

        Uri requestUri = new(url);
        HttpRequestMessage requestMessage = new()
        {
            Method = HttpMethod.Post,
            RequestUri = requestUri,
            Content = httpContent
        };

        return await SendUriAsync<TOut>(requestMessage, apiResult, isAuth, token, isRole);
    }

    private async Task<TOut> SendUriAsync<TOut>(HttpRequestMessage requestMessage, bool apiResult = true, bool isAuth = true, string token = "", bool isRole = true)
    {
        var client = CreateClient(isAuth, token, isRole);
        var result = await client.SendAsync(requestMessage);
        var response = await result.Content.ReadAsStringAsync();

        //result.EnsureSuccessStatusCode();

        if (apiResult)
        {
            var apiResultHttp = JsonConvert.DeserializeObject<ApiResultHttp<TOut>>(response);
            return !apiResultHttp.IsSuccess
                ? throw new WebClientException(apiResultHttp.MetaData.Message)
                : apiResultHttp.Data;
        }
        else
        {
            var apiResultHttp = JsonConvert.DeserializeObject<TOut>(response);

            return apiResultHttp;
        }
    }

    private async Task<MemoryStream> SendUriStreamAsync(HttpRequestMessage requestMessage, bool apiResult = true, bool isAuth = true, string token = "", bool isRole = true)
    {
        using var client = CreateClient(isAuth, token, isRole);

        var result = await client.SendAsync(requestMessage);

        var responseStream = await result.Content.ReadAsStreamAsync();

        var memoryStream = new MemoryStream();
        await responseStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        return memoryStream;
    }

    private HttpClient CreateClient(bool isAuth = true, string token = "", bool isRole = true)
    {
        var client = httpClientFactory.CreateClient(Constants.HttpClientName);

        if (!isAuth)
            return client;

        if (string.IsNullOrWhiteSpace(token))
        {
            token = authService.GetUserToken();
        }

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        if (!isRole)
            return client;

        var roleId = authService.GetRoleId();
        client.DefaultRequestHeaders.Add(Constants.RoleIdKey, roleId.ToString());

        return client;
    }

    private static bool IsSimpleType(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return type.IsPrimitive
               || type.IsEnum
               || type == typeof(string)
               || type == typeof(decimal)
               || type == typeof(DateTime)
               || type == typeof(DateTimeOffset)
               || type == typeof(Guid)
               || type == typeof(TimeSpan);
    }

    private static void AddToContent(MultipartFormDataContent content, object? value, string fieldName, Dictionary<Type, PropertyInfo[]>? localCache = null)
    {
        if (value == null)
            return;

        localCache ??= new Dictionary<Type, PropertyInfo[]>();

        switch (value)
        {
            case IFormFile formFile:
                {
                    var streamContent = new StreamContent(formFile.OpenReadStream());
                    streamContent.Headers.ContentType =
                        new MediaTypeHeaderValue(formFile.ContentType ?? "application/octet-stream");
                    content.Add(streamContent, fieldName, formFile.FileName);
                    return;
                }
            case IBrowserFile browserFile:
                {
                    var stream = browserFile.OpenReadStream(long.MaxValue);
                    var streamContent = new StreamContent(stream);
                    streamContent.Headers.ContentType =
                        new MediaTypeHeaderValue(browserFile.ContentType ?? "application/octet-stream");
                    content.Add(streamContent, fieldName, browserFile.Name);
                    return;
                }
        }

        var valueType = Nullable.GetUnderlyingType(value.GetType()) ?? value.GetType();

        if (IsSimpleType(valueType))
        {
            HttpContent httpContent = valueType switch
            {
                _ when valueType.IsPrimitive || valueType == typeof(decimal) || valueType == typeof(bool) =>
                    new StringContent(Convert.ToString(value, CultureInfo.InvariantCulture)),
                _ when valueType == typeof(DateTime) =>
                    new StringContent(((DateTime)value).ToString("o")), // ISO 8601
                _ when valueType == typeof(DateTimeOffset) =>
                    new StringContent(((DateTimeOffset)value).ToString("o")),
                _ when valueType == typeof(TimeSpan) =>
                    new StringContent(value.ToString() ?? string.Empty),
                _ when valueType == typeof(Guid) =>
                    new StringContent(value.ToString() ?? string.Empty),
                _ => new StringContent(value.ToString() ?? string.Empty)
            };

            content.Add(httpContent, fieldName);
            return;
        }

        if (value is System.Collections.IEnumerable enumerable && value is not string)
        {
            var index = 0;
            foreach (var item in enumerable)
            {
                var indexedName = $"{fieldName}[{index}]";
                AddToContent(content, item, indexedName, localCache);
                index++;
            }
            return;
        }

        if (!localCache.TryGetValue(valueType, out var props))
        {
            props = valueType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            localCache[valueType] = props;
        }

        foreach (var prop in props)
        {
            var propValue = prop.GetValue(value);
            if (propValue == null)
                continue;

            var nestedName = $"{fieldName}.{prop.Name}";
            AddToContent(content, propValue, nestedName, localCache);
        }
    }
}