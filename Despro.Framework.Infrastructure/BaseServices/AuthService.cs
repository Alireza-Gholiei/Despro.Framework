using System.IdentityModel.Tokens.Jwt;
using Despro.Framework.Base.IBaseServices;
using Despro.Framework.Infrastructure.InfrastructureExceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using System.Text;

namespace Despro.Framework.Infrastructure.BaseServices;

public class AuthService(
    IHttpContextAccessor httpContextAccessor,
    IConfiguration configuration) : IAuthService
{
    public long GetUserId()
    {
        try
        {
            var userId = httpContextAccessor.HttpContext?.User.Claims.First(a => a.Type == "Id").Value ?? "0";
            return long.Parse(userId);
        }
        catch (Exception)
        {
            return 0;
        }
    }

    public string GetUserFullName()
    {
        try
        {
            if (!GetClaims().Any())
            {
                return "کاربر مهمان";
            }
            var fullName = GetClaims().FirstOrDefault(a => a.Type == "FullName")?.Value ?? "کاربر مهمان";

            return fullName;
        }
        catch (Exception)
        {
            return "کاربر مهمان";
        }
    }

    public Guid? GetUserGuid()
    {
        try
        {
            var userGuid = GetClaims().FirstOrDefault(a => a.Type == "UserGuid")?.Value ?? Guid.Empty.ToString();

            return Guid.Parse(userGuid);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public string GetUserToken()
    {
        try
        {
            if (httpContextAccessor.HttpContext == null)
                return "";

            var token = httpContextAccessor.HttpContext?.Request.Headers[HeaderNames.Authorization].ToString() ?? "";

            token = !string.IsNullOrWhiteSpace(token)
                ? token.Replace("Bearer ", "")
                : "";

            return token;
        }
        catch (Exception)
        {
            return "";
        }
    }

    public long GetRoleId()
    {
        try
        {
            var roleIdFirst = httpContextAccessor.HttpContext?.Request.Headers["RoleId"].FirstOrDefault() ?? "0";
            var roleId = long.Parse(roleIdFirst);

            CheckRoleValidation(roleId);
            // For Valid Role By Guid
            GetRoleGuid();

            return roleId;
        }
        catch (AuthException)
        {
            throw;
        }
        catch (Exception)
        {
            return 0;
        }
    }

    public Guid GetRoleGuid()
    {
        try
        {
            var roleFirst = httpContextAccessor.HttpContext?.Request.Headers["RoleGuid"].FirstOrDefault() ?? Guid.Empty.ToString();

            var roleGuid = Guid.Parse(roleFirst);

            CheckRoleValidationByRoleGuid(roleGuid);

            return roleGuid;
        }
        catch (AuthException)
        {
            throw;
        }
        catch (Exception)
        {
            return Guid.Empty;
        }
    }

    public List<Guid> GetRolesGuid()
    {
        try
        {
            var roles = GetClaims()
                .Where(a => a.Type == "RolesGuid")
                .Select(claim => Guid.Parse(claim.Value))
                .ToList();

            return roles;
        }
        catch (Exception)
        {
            return [];
        }
    }

    public List<long> GetRolesLong()
    {
        try
        {
            var rolesStr = httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(a => a.Type == "RolesId")?.Value ?? "[]";

            var roles = JsonConvert.DeserializeObject<List<RoleAuthService>>(rolesStr)!;

            return roles.Select(x => x.RoleId).ToList();
        }
        catch (Exception)
        {
            return [];
        }
    }

    public string GetRoleExpDate()
    {
        var roleExpDate = GetClaims().FirstOrDefault(a => a.Type == "RoleExpireDate")?.Value ?? "0";

        return roleExpDate;
    }

    public long GetExpDate()
    {
        try
        {
            var expires = GetClaims().FirstOrDefault(a => a.Type == "ExpireDate")?.Value ?? "0";

            return long.Parse(expires);
        }
        catch (Exception)
        {
            return 0;
        }
    }

    public long GetExpires()
    {
        try
        {
            var expires = GetClaims().FirstOrDefault(a => a.Type == "Expires")?.Value ?? "0";

            return long.Parse(expires);
        }
        catch (Exception)
        {
            return 0;
        }
    }

    public IEnumerable<Claim> GetClaims()
    {
        var accessToken = GetUserToken();

        if (string.IsNullOrEmpty(accessToken))
            return [];

        SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(configuration["JwtConfig:SignInKey"] ?? string.Empty));
        JwtSecurityTokenHandler tokenHandler = new();

        TokenValidationParameters validationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["JwtConfig:Issuer"],
            ValidAudience = configuration["JwtConfig:Audience"],
            IssuerSigningKey = securityKey
        };

        try
        {
            tokenHandler.ValidateToken(accessToken, validationParameters, out _);

            if (tokenHandler.ReadToken(accessToken) is JwtSecurityToken jsonToken)
            {
                return jsonToken.Claims;
            }
        }
        catch (Exception)
        {
            return [];
        }

        return [];
    }

    public string GetIpAddress()
    {
        var context = httpContextAccessor.HttpContext;

        var ip = context?.Request.Headers["X-Forwarded-For"].FirstOrDefault();

        if (!string.IsNullOrEmpty(ip))
        {
            return ip.Split(',').First().Trim();
        }

        ip = context?.Connection.RemoteIpAddress?.ToString();

        return ip ?? "Unknown";
    }

    public void CheckRoleValidation(long roleId)
    {
        var userRoles = GetRolesLong();

        var exist = userRoles.Any(r => r == roleId);

        if (!exist)
        {
            throw new AuthException("نقش کاربر معتبر نیست");
        }
    }

    public void CheckRoleValidationByRoleGuid(Guid roleGuid)
    {
        var roleExpDateJson = GetRoleExpDate();

        var jArray = JArray.Parse(roleExpDateJson);

        var roleExpDate = jArray.FirstOrDefault(token => token["RoleGuid"]?.ToString() == roleGuid.ToString());

        var expDate = long.Parse(roleExpDate?["ExpireDate"]?.ToString() ?? "0");
        var nowDate = DateTime.UtcNow.Ticks;

        if (expDate > 0 && nowDate > expDate)
        {
            throw new AuthException("نقش کاربر معتبر نیست");
        }

        var userRoles = GetRolesGuid();
        var exist = userRoles.Any(r => r == roleGuid);
        if (!exist)
        {
            throw new AuthException("نقش کاربر معتبر نیست");
        }
    }
}

internal record RoleAuthService
{
    public long RoleId { get; set; }
    public Guid RoleGuid { get; set; }
}