namespace Despro.Framework.Base.IBaseServices;

public interface IErrorLogger : IDisposable, IAsyncDisposable
{
    Task<bool> LogError(Exception error, object? Data = null);
    string GetErrorFilePath(DateTime dateTime);
}