namespace Despro.Framework.Base.BaseModels;

public class SystemError : AggregateRoot
{
    public string UserId { get; set; } = string.Empty;
    public string UserFullName { get; set; } = string.Empty;
    public long? Role { get; set; }
    public string UserRoles { get; set; } = string.Empty;
    public string PersianDate { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? InnerExceptionMessage { get; set; } = string.Empty;
    public string? Data { get; set; }
    public string ErrorFile { get; set; } = string.Empty;
}