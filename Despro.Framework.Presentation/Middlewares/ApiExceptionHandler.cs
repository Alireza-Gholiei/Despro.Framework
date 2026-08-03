using Despro.Framework.Base.BaseExceptions;
using Despro.Framework.Base.IBaseServices;
using Despro.Framework.Presentation.ControllerTools;
using Despro.Framework.Presentation.PresentationApiExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Despro.Framework.Presentation.Middlewares;

public static class ApiExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseApiExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ApiExceptionHandlerMiddleware>();
    }
}

public class ApiExceptionHandlerMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context, IErrorLogger errorLogger)
    {
        string? message = null;
        AppStatusCode apiStatusCode;

        try
        {
            await next(context);
        }
        catch (BaseException exception)
        {
            apiStatusCode = AppStatusCode.BadRequest;
            SetErrorMessage(exception);
            await WriteToResponseAsync();
        }
        catch (BaseForbiddenException exception)
        {
            apiStatusCode = AppStatusCode.Forbidden;
            SetErrorMessage(exception);
            await WriteToResponseAsync();
        }
        catch (BaseNotFoundException exception)
        {
            apiStatusCode = AppStatusCode.NotFound;
            SetErrorMessage(exception);
            await WriteToResponseAsync();
        }
        catch (BaseInvalidDataException exception)
        {
            apiStatusCode = AppStatusCode.InvalidData;
            SetErrorMessage(exception);
            await WriteToResponseAsync();
        }
        catch (Exception exception)
        {
            await errorLogger.LogError(exception);
            apiStatusCode = AppStatusCode.ServerError;
            SetErrorMessage(new Exception("خطای ناشناخته در سیستم رخ داده است."));
            await WriteToResponseAsync();
        }

        return;

        void SetErrorMessage(Exception exception)
        {
            message = exception.Message;
        }

        async Task WriteToResponseAsync()
        {
            if (context.Response.HasStarted)
            {
                throw new InvalidOperationException("پاسخ از قبل شروع شده است، میان افزار کد وضعیت درخواست اجرا نخواهد شد.");
            }

            var result = new ApiResult(false, new MetaData(message, apiStatusCode));

            var json = JsonConvert.SerializeObject(result);

            context.Response.StatusCode = (int)apiStatusCode.GetStatusCode();
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(json);
        }
    }
}