namespace Despro.Framework.Base.BaseModels.HttpModels;

public class ApiResultHttp
{
    public bool IsSuccess { get; set; }
    public MetaDataHttp MetaData { get; set; }
}
public class ApiResultHttp<TData>
{
    public bool IsSuccess { get; set; }
    public TData Data { get; set; }
    public MetaDataHttp MetaData { get; set; }
}
public class MetaDataHttp
{
    public string? Message { get; set; }
    public AppStatusCodeHttp AppStatusCode { get; set; }
}

public enum AppStatusCodeHttp
{
    Success = 200,
    NoContent = 204,
    BadRequest = 400,
    UnAuthorize = 401,
    NotFound = 404,
    InvalidData = 422,
    ServerError = 500,
    LogicError = 500,
}