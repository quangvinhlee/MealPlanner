using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MealPlannerApp.Services;
using MealPlannerApp.DTOs;

namespace MealPlannerApp.Controllers
{
    /// <summary>
    /// Controller for managing recipes
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RecipesController : ControllerBase
    {
        private readonly RecipeService _recipeService;
        private readonly ILogger<RecipesController> _logger;

        public RecipesController(RecipeService recipeService, ILogger<RecipesController> logger)
        {
            _recipeService = recipeService;
            _logger = logger;
        }

        /// <summary>
        /// Get all recipes
        /// </summary>
        /// <returns>List of all recipes</returns>
        /// <response code="200">Returns the list of recipes</response>
        /// <response code="401">If user is not authenticated</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<RecipeResponseDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<List<RecipeResponseDto>>> GetAllRecipes()
        {
            try
            {
                _logger.LogInformation("Getting all recipes");
                var recipes = await _recipeService.GetAllAsync();
                _logger.LogInformation("Successfully retrieved {Count} recipes", recipes.Count);
                return Ok(recipes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all recipes");
                return StatusCode(500, "An error occurred while retrieving recipes");
            }
        }

        /// <summary>
        /// Get recipe by ID
        /// </summary>
        /// <param name="id">Recipe ID</param>
        /// <returns>Recipe details</returns>
        /// <response code="200">Returns the recipe</response>
        /// <response code="404">If recipe is not found</response>
        /// <response code="401">If user is not authenticated</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(RecipeResponseDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<RecipeResponseDto>> GetRecipe(Guid id)
        {
            try
            {
                _logger.LogInformation("Getting recipe with ID: {RecipeId}", id);
                var recipe = await _recipeService.GetByIdAsync(id);

                if (recipe == null)
                {
                    _logger.LogWarning("Recipe not found with ID: {RecipeId}", id);
                    return NotFound($"Recipe with ID {id} not found");
                }

                _logger.LogInformation("Successfully retrieved recipe: {RecipeName}", recipe.Name);
                return Ok(recipe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting recipe with ID: {RecipeId}", id);
                return StatusCode(500, "An error occurred while retrieving the recipe");
            }
        }

        /// <summary>
        /// Create a new recipe
        /// </summary>
        /// <param name="createDto">Recipe creation data</param>
        /// <returns>Created recipe</returns>
        /// <response code="201">Recipe created successfully</response>
        /// <response code="400">If the recipe data is invalid</response>
        /// <response code="401">If user is not authenticated</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpPost]
        [ProducesResponseType(typeof(RecipeResponseDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<RecipeResponseDto>> CreateRecipe([FromBody] RecipeCreateDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for recipe creation");
                    return BadRequest(ModelState);
                }

                // Get current user ID from JWT claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    _logger.LogWarning("Invalid or missing user ID in token");
                    return Unauthorized("Invalid user token");
                }

                _logger.LogInformation("Creating recipe: {RecipeName} for user: {UserId}", createDto.Name, userId);
                var recipe = await _recipeService.CreateAsync(createDto, userId);

                _logger.LogInformation("Successfully created recipe: {RecipeName} with ID: {RecipeId}", recipe.Name, recipe.Id);
                return CreatedAtAction(nameof(GetRecipe), new { id = recipe.Id }, recipe);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for recipe creation");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating recipe: {RecipeName}", createDto.Name);
                return StatusCode(500, "An error occurred while creating the recipe");
            }
        }

        /// <summary>
        /// Update an existing recipe
        /// </summary>
        /// <param name="id">Recipe ID</param>
        /// <param name="updateDto">Recipe update data</param>
        /// <returns>Updated recipe</returns>
        /// <response code="200">Recipe updated successfully</response>
        /// <response code="404">If recipe is not found</response>
        /// <response code="400">If the recipe data is invalid</response>
        /// <response code="401">If user is not authenticated</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(RecipeResponseDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<RecipeResponseDto>> UpdateRecipe(Guid id, [FromBody] RecipeUpdateDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for recipe update");
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Updating recipe with ID: {RecipeId}", id);
                var recipe = await _recipeService.UpdateAsync(id, updateDto);

                if (recipe == null)
                {
                    _logger.LogWarning("Recipe not found for update with ID: {RecipeId}", id);
                    return NotFound($"Recipe with ID {id} not found");
                }

                _logger.LogInformation("Successfully updated recipe: {RecipeName}", recipe.Name);
                return Ok(recipe);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for recipe update");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating recipe with ID: {RecipeId}", id);
                return StatusCode(500, "An error occurred while updating the recipe");
            }
        }

        /// <summary>
        /// Delete a recipe
        /// </summary>
        /// <param name="id">Recipe ID</param>
        /// <returns>No content</returns>
        /// <response code="204">Recipe deleted successfully</response>
        /// <response code="404">If recipe is not found</response>
        /// <response code="401">If user is not authenticated</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteRecipe(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting recipe with ID: {RecipeId}", id);
                var deleted = await _recipeService.DeleteAsync(id);

                if (!deleted)
                {
                    _logger.LogWarning("Recipe not found for deletion with ID: {RecipeId}", id);
                    return NotFound($"Recipe with ID {id} not found");
                }

                _logger.LogInformation("Successfully deleted recipe with ID: {RecipeId}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting recipe with ID: {RecipeId}", id);
                return StatusCode(500, "An error occurred while deleting the recipe");
            }
        }

        /// <summary>
        /// Get current user's saved recipes
        /// </summary>
        /// <returns>List of user's saved recipes</returns>
        /// <response code="200">Returns the list of saved recipes</response>
        /// <response code="401">If user is not authenticated</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("saved")]
        [ProducesResponseType(typeof(List<RecipeResponseDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<List<RecipeResponseDto>>> GetUserSavedRecipes()
        {
            try
            {
                // Get current user ID from JWT claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    _logger.LogWarning("Invalid or missing user ID in token");
                    return Unauthorized("Invalid user token");
                }

                _logger.LogInformation("Getting saved recipes for user: {UserId}", userId);
                var recipes = await _recipeService.GetUserSavedRecipesAsync(userId);

                _logger.LogInformation("Successfully retrieved {Count} saved recipes for user: {UserId}", recipes.Count, userId);
                return Ok(recipes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user's saved recipes");
                return StatusCode(500, "An error occurred while retrieving saved recipes");
            }
        }
    }
}