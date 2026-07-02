namespace Despro.Framework.Base.BaseModels;

public enum OperationResultStatus
{
    Success = 200,
    NoContent = 204,
    Error = 400,
    UnAuthorize = 401,
    Forbidden = 403,
    NotFound = 404
}

public abstract class OperationResultBase
{
    private const string SuccessMessage = "عملیات با موفقیت انجام شد";
    private const string ErrorMessage = "خطایی در انجام عملیات رخ داده است";
    private const string NotFoundMessage = "اطلاعات یافت نشد";
    private const string UnAuthorizeMessage = "لطفا وارد حساب کاربری خود شوید";
    private const string ForbiddenMessage = "دسترسی غیرمجاز";

    public string Message { get; set; } = "";
    public string? Title { get; set; }
    public OperationResultStatus Status { get; set; }

    protected static string GetDefaultMessage(OperationResultStatus status) => status switch
    {
        OperationResultStatus.Success => SuccessMessage,
        OperationResultStatus.NoContent => SuccessMessage,
        OperationResultStatus.Error => ErrorMessage,
        OperationResultStatus.NotFound => NotFoundMessage,
        OperationResultStatus.UnAuthorize => UnAuthorizeMessage,
        OperationResultStatus.Forbidden => ForbiddenMessage,
        _ => ErrorMessage
    };
}

public class OperationResult : OperationResultBase
{
    private OperationResult() { }

    public static OperationResult Create(OperationResultStatus status, string? message = null, string? title = null)
    {
        return new OperationResult
        {
            Status = status,
            Message = message ?? GetDefaultMessage(status),
            Title = title
        };
    }

    public static OperationResult Success(string? message = null, string? title = null) => Create(OperationResultStatus.Success, message, title);
    public static OperationResult Error(string? message = null, string? title = null) => Create(OperationResultStatus.Error, message, title);
    public static OperationResult NotFound(string? message = null, string? title = null) => Create(OperationResultStatus.NotFound, message, title);
    public static OperationResult NoContent(string? message = null, string? title = null) => Create(OperationResultStatus.NoContent, message, title);
    public static OperationResult UnAuthorize(string? message = null, string? title = null) => Create(OperationResultStatus.UnAuthorize, message, title);
    public static OperationResult Forbidden(string? message = null, string? title = null) => Create(OperationResultStatus.Forbidden, message, title);
}

public class OperationResult<TData> : OperationResultBase
{
    private OperationResult() { }

    public TData? Data { get; set; }

    public static OperationResult<TData> Create(OperationResultStatus status, TData? data = default, string? message = null, string? title = null)
    {
        return new OperationResult<TData>
        {
            Status = status,
            Data = status == OperationResultStatus.Success ? data : default,
            Message = message ?? GetDefaultMessage(status),
            Title = title
        };
    }

    public static OperationResult<TData> Success(TData data, string? message = null, string? title = null) => Create(OperationResultStatus.Success, data, message, title);
    public static OperationResult<TData> Error(string? message = null, string? title = null) => Create(OperationResultStatus.Error, default, message, title);
    public static OperationResult<TData> NotFound(string? message = null, string? title = null) => Create(OperationResultStatus.NotFound, default, message, title);
    public static OperationResult<TData> NoContent(string? message = null, string? title = null) => Create(OperationResultStatus.NoContent, default, message, title);
    public static OperationResult<TData> UnAuthorize(string? message = null, string? title = null) => Create(OperationResultStatus.UnAuthorize, default, message, title);
    public static OperationResult<TData> Forbidden(string? message = null, string? title = null) => Create(OperationResultStatus.Forbidden, default, message, title);
}

public class OperationQueryResult : OperationResultBase
{
    private OperationQueryResult() { }

    public static OperationQueryResult Create(OperationResultStatus status, string? message = null, string? title = null)
    {
        return new OperationQueryResult
        {
            Status = status,
            Message = message ?? GetDefaultMessage(status),
            Title = title
        };
    }

    public static OperationQueryResult Success(string? message = null, string? title = null) => Create(OperationResultStatus.Success, message, title);
    public static OperationQueryResult Error(string? message = null, string? title = null) => Create(OperationResultStatus.Error, message, title);
    public static OperationQueryResult NotFound(string? message = null, string? title = null) => Create(OperationResultStatus.NotFound, message, title);
    public static OperationQueryResult NoContent(string? message = null, string? title = null) => Create(OperationResultStatus.NoContent, message, title);
    public static OperationQueryResult UnAuthorize(string? message = null, string? title = null) => Create(OperationResultStatus.UnAuthorize, message, title);
    public static OperationQueryResult Forbidden(string? message = null, string? title = null) => Create(OperationResultStatus.Forbidden, message, title);
}

public class OperationQueryResult<TData> : OperationResultBase
{
    private OperationQueryResult() { }

    public TData? Data { get; set; }

    public static OperationQueryResult<TData> Create(OperationResultStatus status, TData? data = default, string? message = null, string? title = null)
    {
        return new OperationQueryResult<TData>
        {
            Status = status,
            Data = status == OperationResultStatus.Success ? data : default,
            Message = message ?? GetDefaultMessage(status),
            Title = title
        };
    }

    public static OperationQueryResult<TData> Success(TData data, string? message = null, string? title = null) => Create(OperationResultStatus.Success, data, message, title);
    public static OperationQueryResult<TData> Error(string? message = null, string? title = null) => Create(OperationResultStatus.Error, default, message, title);
    public static OperationQueryResult<TData> NotFound(string? message = null, string? title = null) => Create(OperationResultStatus.NotFound, default, message, title);
    public static OperationQueryResult<TData> NoContent(string? message = null, string? title = null) => Create(OperationResultStatus.NoContent, default, message, title);
    public static OperationQueryResult<TData> UnAuthorize(string? message = null, string? title = null) => Create(OperationResultStatus.UnAuthorize, default, message, title);
    public static OperationQueryResult<TData> Forbidden(string? message = null, string? title = null) => Create(OperationResultStatus.Forbidden, default, message, title);
}