using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MealPlannerApp.Controllers
{
    public static class ControllerBaseExtensions
    {
        public static Guid? GetUserId(this ControllerBase controller)
        {
            return Guid.TryParse(controller.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;
        }
    }
}