using Despro.Framework.Base.BaseModels;
using Despro.Framework.Presentation.ControllerTools;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Net;

namespace Despro.Framework.Presentation.PresentationApiExtensions;

public static class HttpEnumHelper
{
    public static AppStatusCode MapOperationStatus(this OperationResultStatus status) =>
        status switch
        {
            OperationResultStatus.Success => AppStatusCode.Success,
            OperationResultStatus.NoContent => AppStatusCode.NoContent,
            OperationResultStatus.NotFound => AppStatusCode.NotFound,
            OperationResultStatus.Error => AppStatusCode.BadRequest,
            OperationResultStatus.UnAuthorize => AppStatusCode.UnAuthorize,
            OperationResultStatus.Forbidden => AppStatusCode.Forbidden,
            _ => AppStatusCode.ServerError
        };

    extension(AppStatusCode appStatusCode)
    {
        public Results<Ok<ApiResult<TData>>, NoContent, BadRequest<ApiResult<TData>>, UnauthorizedHttpResult, ForbidHttpResult, NotFound<ApiResult<TData>>> GetHttpStatusCode<TData>(ApiResult<TData> result) =>
            appStatusCode switch
            {
                AppStatusCode.Success => TypedResults.Ok(result),
                AppStatusCode.NoContent => TypedResults.NoContent(),
                AppStatusCode.BadRequest or AppStatusCode.InvalidData => TypedResults.BadRequest(result),
                AppStatusCode.UnAuthorize => TypedResults.Unauthorized(),
                AppStatusCode.Forbidden => TypedResults.Forbid(),
                AppStatusCode.NotFound => TypedResults.NotFound(result),
                _ => TypedResults.BadRequest(result)
            };

        public Results<Ok<ApiResult>, NoContent, BadRequest<ApiResult>, UnauthorizedHttpResult, ForbidHttpResult, NotFound<ApiResult>> GetHttpStatusCode(ApiResult result) =>
            appStatusCode switch
            {
                AppStatusCode.Success => TypedResults.Ok(result),
                AppStatusCode.NoContent => TypedResults.NoContent(),
                AppStatusCode.BadRequest or AppStatusCode.InvalidData => TypedResults.BadRequest(result),
                AppStatusCode.UnAuthorize => TypedResults.Unauthorized(),
                AppStatusCode.Forbidden => TypedResults.Forbid(),
                AppStatusCode.NotFound => TypedResults.NotFound(result),
                _ => TypedResults.BadRequest(result)
            };

        public int GetHttpStatusCode() =>
            appStatusCode switch
            {
                AppStatusCode.Success => StatusCodes.Status200OK,
                AppStatusCode.NoContent => StatusCodes.Status204NoContent,
                AppStatusCode.BadRequest or AppStatusCode.InvalidData => StatusCodes.Status400BadRequest,
                AppStatusCode.UnAuthorize => StatusCodes.Status401Unauthorized,
                AppStatusCode.Forbidden => StatusCodes.Status403Forbidden,
                AppStatusCode.NotFound => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError
            };

        public HttpStatusCode GetStatusCode() =>
            appStatusCode switch
            {
                AppStatusCode.Success => HttpStatusCode.OK,
                AppStatusCode.NoContent => HttpStatusCode.NoContent,
                AppStatusCode.InvalidData or AppStatusCode.BadRequest => HttpStatusCode.BadRequest,
                AppStatusCode.UnAuthorize => HttpStatusCode.Unauthorized,
                AppStatusCode.Forbidden => HttpStatusCode.Forbidden,
                AppStatusCode.NotFound => HttpStatusCode.NotFound,
                _ => HttpStatusCode.InternalServerError
            };
    }
}