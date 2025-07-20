using Microsoft.AspNetCore.Mvc;
using MealPlannerApp.Services;
using MealPlannerApp.DTOs;

namespace MealPlannerApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SpoonacularController : ControllerBase
    {
        private readonly SpoonacularService _spoonacularService;

        public SpoonacularController(SpoonacularService spoonacularService)
        {
            _spoonacularService = spoonacularService;
        }

        /// <summary>
        /// Get recipes based on search criteria
        /// </summary>
        [HttpGet("recipes")]
        public async Task<IActionResult> GetRecipes([FromQuery] GetRecipesDto dto)
        {
            try
            {
                var recipes = await _spoonacularService.GetRecipes(dto);
                return Ok(recipes);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", detail = ex.Message });
            }
        }
    }
}