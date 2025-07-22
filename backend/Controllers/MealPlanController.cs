using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MealPlannerApp.Services;
using MealPlannerApp.DTOs;
using System;
using System.Threading.Tasks;

namespace MealPlannerApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MealPlanController : ControllerBase
    {
        private readonly MealPlanService _mealPlanService;

        public MealPlanController(MealPlanService mealPlanService)
        {
            _mealPlanService = mealPlanService;
        }

        [HttpPost]
        public async Task<IActionResult> SaveMealPlan([FromBody] MealPlanCreateDto dto)
        {
            var userId = this.GetUserId();
            if (userId == null)
                return Unauthorized();

            var mealPlan = await _mealPlanService.SaveMealPlanAsync(userId.Value, dto);
            return Ok(mealPlan);
        }

        [HttpGet]
        public async Task<IActionResult> GetMealPlans()
        {
            var userId = this.GetUserId();
            if (userId == null)
                return Unauthorized();

            var mealPlans = await _mealPlanService.GetMealPlansForUserAsync(userId.Value);
            return Ok(mealPlans);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMealPlan(Guid id)
        {
            var userId = this.GetUserId();
            if (userId == null)
                return Unauthorized();

            var result = await _mealPlanService.DeleteMealPlanAsync(userId.Value, id);
            if (!result)
                return NotFound();
            return NoContent();
        }
    }
}
