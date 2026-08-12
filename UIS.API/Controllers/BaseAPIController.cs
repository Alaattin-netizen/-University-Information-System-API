using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace UIS.API.Controllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst("nameid")?.Value;
        return int.TryParse(userIdClaim, out var id) ? id : 0;
    }

    protected string GetCurrentUserEmail()
    {
        return User.FindFirst(ClaimTypes.Email)?.Value
               ?? User.FindFirst("email")?.Value
               ?? "Unknown";
    }

    protected List<string> GetCurrentUserRoles()
    {
        return User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
    }
    protected string GetClientIp()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
}