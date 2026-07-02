using System.Security.Claims;

namespace Despro.Framework.Base.IBaseServices;

public interface IAuthService
{
    long GetUserId();
    string GetUserFullName();
    Guid? GetUserGuid();
    string GetUserToken();
    long GetRoleId();
    Guid GetRoleGuid();
    List<Guid> GetRolesGuid();
    List<long> GetRolesLong();
    string GetRoleExpDate();
    long GetExpDate();
    long GetExpires();
    IEnumerable<Claim> GetClaims();
    string GetIpAddress();
    void CheckRoleValidation(long RoleID);
    void CheckRoleValidationByRoleGuid(Guid RoleGuid);
}