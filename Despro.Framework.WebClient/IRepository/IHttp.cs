using Microsoft.AspNetCore.Http;

namespace Despro.Framework.WebClient.IRepository;

public interface IHttp
{
    Task<TOut> GetAsync<TOut>(string url, bool apiResult = true, bool isAuth = true, string token = "", bool isRole = true);
    Task<TOut> PostAsync<TIn, TOut>(string url, TIn model, bool apiResult = true, bool isAuth = true, string token = "", bool isRole = true);
    Task<TOut> PostFormAsync<TIn, TOut>(string url, TIn model, bool apiResult = true, bool isAuth = true, string token = "", bool isRole = true);
    Task<TOut> PutAsync<TIn, TOut>(string url, TIn model, bool apiResult = true, bool isAuth = true, string token = "", bool isRole = true);
    Task<TOut> PutFormAsync<TIn, TOut>(string url, TIn model, bool apiResult = true, bool isAuth = true, string token = "", bool isRole = true);
    Task<TOut> PatchAsync<TIn, TOut>(string url, TIn model, bool apiResult = true, bool isAuth = true, string token = "", bool isRole = true);
    Task<TOut> PatchFormAsync<TIn, TOut>(string url, TIn model, bool apiResult = true, bool isAuth = true, string token = "", bool isRole = true);
    Task<TOut> DeleteAsync<TOut>(string url, bool apiResult = true, bool isAuth = true, string token = "", bool isRole = true);
    Task<MemoryStream> GetFileAsync(string url, bool apiResult = true, bool isAuth = true, string token = "", bool isRole = true);
    Task<TOut> UploadFile<TOut>(string url, IFormFile file, bool apiResult = true, bool isAuth = true, string token = "", bool isRole = true);
}