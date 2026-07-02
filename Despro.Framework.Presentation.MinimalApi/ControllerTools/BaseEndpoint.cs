using Asp.Versioning.Builder;
using Despro.Framework.Base.BaseModels;
using Despro.Framework.Presentation.ControllerTools;
using Despro.Framework.Presentation.MinimalApi.PresentationExceptions;
using Despro.Framework.Presentation.PresentationApiExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Despro.Framework.Presentation.MinimalApi.ControllerTools;

public abstract class BaseEndpoint : IEndpoint
{
    #region Helpers
    private Results<Ok<ApiResult<TData>>, NoContent, BadRequest<ApiResult<TData>>, UnauthorizedHttpResult, ForbidHttpResult, NotFound<ApiResult<TData>>> BuildApiResult<TData>(bool isSuccess, TData data, string message, AppStatusCode status)
    {
        var result = new ApiResult<TData>(isSuccess,
            new MetaData(message, status),
            isSuccess ? data : default);

        return status.GetHttpStatusCode(result);
    }

    private Results<Ok<ApiResult>, NoContent, BadRequest<ApiResult>, UnauthorizedHttpResult, ForbidHttpResult, NotFound<ApiResult>> BuildApiResult(bool isSuccess, string message, AppStatusCode status)
    {
        var result = new ApiResult(isSuccess,
            new MetaData(message, status));

        return status.GetHttpStatusCode(result);
    }
    #endregion

    #region Command
    protected Results<Ok<ApiResult<TData>>, NoContent, BadRequest<ApiResult<TData>>, UnauthorizedHttpResult, ForbidHttpResult, NotFound<ApiResult<TData>>> CommandResult<TData>(OperationResult<TData> result)
    {
        var isSuccess = result.Status == OperationResultStatus.Success;
        return BuildApiResult(isSuccess, result.Data, result.Message, result.Status.MapOperationStatus());
    }

    protected Results<Ok<ApiResult>, NoContent, BadRequest<ApiResult>, UnauthorizedHttpResult, ForbidHttpResult, NotFound<ApiResult>> CommandResult(OperationResult result)
    {
        var isSuccess = result.Status == OperationResultStatus.Success;
        return BuildApiResult(isSuccess, result.Message, result.Status.MapOperationStatus());
    }
    #endregion

    #region Query
    protected Results<Ok<ApiResult<TData>>, NoContent, BadRequest<ApiResult<TData>>, UnauthorizedHttpResult, ForbidHttpResult, NotFound<ApiResult<TData>>> QueryResult<TData>(TData result)
    {
        return BuildApiResult(true, result, "عملیات با موفقیت انجام شد", AppStatusCode.Success);
    }

    protected Results<Ok<ApiResult<TData>>, NoContent, BadRequest<ApiResult<TData>>, UnauthorizedHttpResult, ForbidHttpResult, NotFound<ApiResult<TData>>> QueryResult<TData>(OperationQueryResult<TData> result)
    {
        var isSuccess = result.Status == OperationResultStatus.Success;
        return BuildApiResult(isSuccess, result.Data, result.Message, result.Status.MapOperationStatus());
    }
    #endregion

    public virtual string? Route => null;
    public virtual string? Tag => null;
    public virtual string? GroupName => null;
    public virtual double Version => 1.0;

    public void MapEndpoint(IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        string finalTag;
        string groupPath;

        if (!string.IsNullOrWhiteSpace(Tag))
        {
            finalTag = Tag;
        }
        else
        {
            var className = GetType().Name;
            if (!className.EndsWith("Endpoints"))
                throw new PresentationException($"Endpoint Class Name '{className}' Must End With 'Endpoints'");

            finalTag = className.Replace("Endpoints", "");
        }

        if (!string.IsNullOrWhiteSpace(Route) || Route != null)
        {
            groupPath = Route;
        }
        else
        {
            var rootPrefix = FrameworkPresentationWebDi._routePrefix.Replace("/[controller]", "");
            groupPath = FrameworkPresentationWebDi._routePrefix.Contains("/[controller]")
                ? $"{rootPrefix}/{finalTag}"
                : rootPrefix;
        }

        var group = app.MapGroup(groupPath)
            .WithApiVersionSet(versionSet)
            .HasApiVersion(Version)
            .WithTags(finalTag)
            .WithOpenApi();

        DefineEndpoints(group);
    }


    protected abstract void DefineEndpoints(IEndpointRouteBuilder app);
}