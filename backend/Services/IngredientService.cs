using MealPlannerApp.Models;
using MealPlannerApp.Data;
using Microsoft.EntityFrameworkCore;
using MealPlannerApp.DTOs;

namespace MealPlannerApp.Services
{
    public class IngredientService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<IngredientService> _logger;

        public IngredientService(AppDbContext context, ILogger<IngredientService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<IngredientDto>> GetAllAsync()
        {
            var ingredients = await _context.Ingredients
                .Include(i => i.FridgeItems)
                .Include(i => i.Recipes)
                .ToListAsync();

            // Direct mapping following the actual model properties
            return ingredients.Select(ingredient => new IngredientDto
            {
                Id = ingredient.Id,
                Name = ingredient.Name,
                FridgeItems = ingredient.FridgeItems?.Select(item => new FridgeItemResponseDto
                {
                    Id = item.Id,
                    Name = item.Name,
                    Quantity = item.Quantity,
                    Unit = item.Unit,
                    ExpirationDate = item.ExpirationDate
                }).ToList() ?? new List<FridgeItemResponseDto>(),
                Recipes = ingredient.Recipes?.Select(recipe => new RecipeResponseDto
                {
                    Id = recipe.Id,
                    Name = recipe.Name, // Fixed: Model has Name, not Title
                    Description = recipe.Description,
                    ImageUrl = recipe.ImageUrl, // Fixed: Model has ImageUrl, not Image
                    Steps = recipe.Steps,
                    CreatedAt = recipe.CreatedAt,
                    UpdatedAt = recipe.UpdatedAt,
                    Ingredients = new List<IngredientDto>() // Avoid circular reference
                }).ToList() ?? new List<RecipeResponseDto>()
            }).ToList();
        }

        public async Task<IngredientDto?> GetByIdAsync(Guid id)
        {
            var ingredient = await _context.Ingredients
                .Include(i => i.FridgeItems)
                .Include(i => i.Recipes)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (ingredient == null)
                return null;

            // Debug output
            _logger.LogInformation("Ingredient: {Name} (ID: {Id})", ingredient.Name, ingredient.Id);
            _logger.LogInformation("FridgeItems count: {Count}", ingredient.FridgeItems?.Count ?? 0);
            _logger.LogInformation("Recipes count: {Count}", ingredient.Recipes?.Count ?? 0);

            // Direct mapping following the actual model properties
            return new IngredientDto
            {
                Id = ingredient.Id,
                Name = ingredient.Name,
                FridgeItems = ingredient.FridgeItems?.Select(item => new FridgeItemResponseDto
                {
                    Id = item.Id,
                    Name = item.Name,
                    Quantity = item.Quantity,
                    Unit = item.Unit,
                    ExpirationDate = item.ExpirationDate
                }).ToList() ?? new List<FridgeItemResponseDto>(),
                Recipes = ingredient.Recipes?.Select(recipe => new RecipeResponseDto
                {
                    Id = recipe.Id,
                    Name = recipe.Name, // Fixed: Model has Name, not Title
                    Description = recipe.Description,
                    ImageUrl = recipe.ImageUrl, // Fixed: Model has ImageUrl, not Image
                    Steps = recipe.Steps,
                    CreatedAt = recipe.CreatedAt,
                    UpdatedAt = recipe.UpdatedAt,
                    Ingredients = new List<IngredientDto>() // Avoid circular reference
                }).ToList() ?? new List<RecipeResponseDto>()
            };
        }

        public async Task<Ingredient> AddAsync(Ingredient ingredient)
        {
            _context.Ingredients.Add(ingredient);
            await _context.SaveChangesAsync();
            return ingredient;
        }

        public async Task<bool> UpdateAsync(Ingredient ingredient)
        {
            var existing = await _context.Ingredients.FindAsync(ingredient.Id);
            if (existing == null) return false;

            existing.Name = ingredient.Name;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var ingredient = await _context.Ingredients.FindAsync(id);
            if (ingredient == null) return false;

            _context.Ingredients.Remove(ingredient);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}