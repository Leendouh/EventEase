using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace EventEase.Controllers
{
    [Authorize]
    public abstract class BaseController : Controller
    {
        // Get the current user's ID (string) for CreatedBy fields
        protected string? GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        // Get the current admin's numeric ID (from claims)
        protected int GetCurrentAdminId()
        {
            var adminIdClaim = User.FindFirst("AdminId")?.Value;
            if (int.TryParse(adminIdClaim, out int adminId))
            {
                return adminId;
            }
            return 0;
        }

        protected string GetCurrentUserName()
        {
            return User.Identity?.Name ?? "Unknown";
        }

        protected string GetFormattedAdminId()
        {
            return User.FindFirst("FormattedAdminId")?.Value ?? "Unknown";
        }

        protected bool IsSuperAdmin()
        {
            return User.IsInRole("SuperAdmin");
        }
    }
}