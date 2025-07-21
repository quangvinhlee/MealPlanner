using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MealPlannerApp.Services;
using MealPlannerApp.DTOs;

namespace MealPlannerApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
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
            var userId = this.GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

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

        /// <summary>
        /// Get detailed information for a specific recipe
        /// </summary>
        [HttpGet("recipes/{id}")]
        public async Task<IActionResult> GetRecipeDetails(int id)
        {
            var userId = this.GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            try
            {
                var dto = new GetRecipeDetailsDto { Id = id };
                var recipeDetails = await _spoonacularService.GetRecipeDetails(dto);

                if (recipeDetails == null)
                {
                    return NotFound(new { message = "Recipe not found" });
                }

                return Ok(recipeDetails);
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