using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace HospitalApp.WebAPI.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected Guid GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    protected string GetCurrentUserRole() =>
        User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
}