namespace Despro.Framework.Presentation.ControllerTools;

public class MetaData(string? message, AppStatusCode appStatusCode)
{
    public string? Message { get; private set; } = message;
    public AppStatusCode AppStatusCode { get; private set; } = appStatusCode;
}

public class ApiResult(bool isSuccess, MetaData? metaData)
{
    public bool IsSuccess { get; private set; } = isSuccess;
    public MetaData? MetaData { get; private set; } = metaData;
}

public class ApiResult<TData>(bool isSuccess, MetaData? metaData, TData? data)
{
    public bool IsSuccess { get; private set; } = isSuccess;
    public TData? Data { get; private set; } = data;
    public MetaData? MetaData { get; private set; } = metaData;
}

public enum AppStatusCode
{
    Success = 200,
    NoContent = 204,
    BadRequest = 400,
    UnAuthorize = 401,
    Forbidden = 403,
    NotFound = 404,
    InvalidData = 422,
    ServerError = 500
}