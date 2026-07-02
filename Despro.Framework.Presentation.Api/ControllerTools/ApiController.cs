using Asp.Versioning;
using Despro.Framework.Base.BaseModels;
using Despro.Framework.Presentation.ControllerTools;
using Despro.Framework.Presentation.PresentationApiExtensions;
using Microsoft.AspNetCore.Mvc;

namespace Despro.Framework.Presentation.Api.ControllerTools;

[ApiController]
[ApiVersion(1.0, Deprecated = false)]
public class ApiController : ControllerBase
{
    #region Helpers
    private void SetResponseStatusCode(AppStatusCode appStatusCode)
    {
        HttpContext.Response.StatusCode = appStatusCode.GetHttpStatusCode();
    }

    private ApiResult<TData> BuildApiResult<TData>(bool isSuccess, TData data, string message, AppStatusCode status)
    {
        var result = new ApiResult<TData>(isSuccess,
            new MetaData(message, status),
            isSuccess ? data : default);

        SetResponseStatusCode(status);

        return result;
    }

    private ApiResult BuildApiResult(bool isSuccess, string message, AppStatusCode status)
    {
        var result = new ApiResult(isSuccess,
            new MetaData(message, status));

        SetResponseStatusCode(status);

        return result;
    }
    #endregion

    #region Command
    protected ApiResult CommandResult(OperationResult result)
    {
        var isSuccess = result.Status == OperationResultStatus.Success;
        return BuildApiResult(isSuccess, result.Message, result.Status.MapOperationStatus());
    }

    protected ApiResult<TData?> CommandResult<TData>(OperationResult<TData> result)
    {
        var isSuccess = result.Status == OperationResultStatus.Success;
        return BuildApiResult(isSuccess, result.Data, result.Message, result.Status.MapOperationStatus());
    }
    #endregion

    #region Query
    protected ApiResult<TData> QueryResult<TData>(TData result)
    {
        return BuildApiResult(true, result, "عملیات با موفقیت انجام شد", AppStatusCode.Success);
    }

    protected ApiResult<TData?> QueryResult<TData>(OperationQueryResult<TData> result)
    {
        var isSuccess = result.Status == OperationResultStatus.Success;
        return BuildApiResult(isSuccess, result.Data, result.Message, result.Status.MapOperationStatus());
    }
    #endregion
}