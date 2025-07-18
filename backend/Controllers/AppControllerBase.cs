using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MealPlannerApp.Controllers
{
    public abstract class AppControllerBase : ControllerBase
    {
        protected Guid? UserId =>
            Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;
    }
}