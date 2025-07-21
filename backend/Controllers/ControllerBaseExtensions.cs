using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace MealPlannerApp.Controllers
{
    public static class ControllerBaseExtensions
    {
        public static Guid? GetUserId(this ControllerBase controller)
        {
            // Try JWT standard claim first (what your token generation uses)
            var userIdClaim = controller.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                           ?? controller.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Guid.TryParse(userIdClaim, out var id) ? id : null;
        }
    }
}