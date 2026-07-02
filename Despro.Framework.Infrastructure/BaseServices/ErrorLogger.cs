using Despro.Framework.Base.BaseExtensions;
using Despro.Framework.Base.BaseModels;
using Despro.Framework.Base.IBaseServices;
using Despro.Framework.Infrastructure.Contexts;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System.Dynamic;

namespace Despro.Framework.Infrastructure.BaseServices;

public class ErrorLogger(
    IAuthService authService,
    IWebHostEnvironment environment,
    EfBaseContext efBaseContext)
    : IErrorLogger
{
    async Task<bool> IErrorLogger.LogError(Exception error, object? data)
    {
        try
        {
            var dateTime = DateTime.UtcNow.ToLocalTime();

            var path = GetErrorFilePath(dateTime);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var dateTimeStr = dateTime.ToString("yyyy.MM.dd.HH.mm.ss");
            var fileName = $"{dateTimeStr}-{Guid.NewGuid():N}.json";

            dynamic errorObject = new ExpandoObject();
            SystemError systemError = new();
            #region Exceptions

            errorObject.Exception = error;

            try
            {
                var userId = authService.GetUserId();
                errorObject.UserId = userId;
                systemError.SetCreate(dateTime.Ticks, userId);
            }
            catch
            {
                // ignored
            }

            try
            {
                var userFullName = authService.GetUserFullName();
                errorObject.UserFullName = userFullName;
                systemError.UserFullName = userFullName;
            }
            catch
            {
                // ignored
            }

            try
            {
                var roleId = authService.GetRoleId();
                errorObject.Role = roleId;
                systemError.Role = roleId;
            }
            catch
            {
                // ignored
            }

            try
            {
                var roles = JsonConvert.SerializeObject(authService.GetRolesLong());
                errorObject.UserRoles = roles;
                systemError.UserRoles = roles;
            }
            catch
            {
                // ignored
            }

            try
            {
                errorObject.UserIP = authService.GetIpAddress();
            }
            catch
            {
                // ignored
            }
            #endregion

            if (data != null)
            {
                var jsonData = JsonConvert.SerializeObject(data);
                errorObject.Data = jsonData;
                systemError.Data = jsonData;
            }

            errorObject.CreateDate = dateTime;
            errorObject.PersianDate = dateTime.ToShamsiWhitTime();
            systemError.PersianDate = dateTime.ToShamsiWhitTime();

            systemError.Message = error.Message + " ### " + error.StackTrace;
            systemError.InnerExceptionMessage = error.InnerException?.Message;

            systemError.ErrorFile = fileName;

            await File.WriteAllLinesAsync(Path.Combine(path, fileName), [JsonConvert.SerializeObject(errorObject)]);

            try
            {
                await efBaseContext.SystemError.AddAsync(systemError);

                await efBaseContext.SaveChangesAsync();
            }
            catch
            {
                return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public string GetErrorFilePath(DateTime dateTime)
    {
        try
        {
            var path = environment.ContentRootPath;

            path = Path.Combine(path, "Errors", $"{dateTime.Year.ToString()}.{dateTime.Month.ToString()}.{dateTime.Day.ToString()}");

            return path;
        }
        catch (Exception)
        {
            return "";
        }
    }

    public void Dispose()
    {
        efBaseContext.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await efBaseContext.DisposeAsync();
    }
}