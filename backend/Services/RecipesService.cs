using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MealPlannerApp.Data;
using MealPlannerApp.DTOs;
using MealPlannerApp.Models;
using Microsoft.EntityFrameworkCore;

namespace MealPlannerApp.Services
{
    public class RecipeService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RecipeService> _logger;

        public RecipeService(AppDbContext context, ILogger<RecipeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<RecipeResponseDto>> GetAllAsync()
        {
            var recipes = await _context.Recipes
                .Include(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Ingredient)
                .ToListAsync();

            // Direct mapping following the actual model properties
            return recipes.Select(recipe => new RecipeResponseDto
            {
                Id = recipe.Id,
                Name = recipe.Name, // Model has Name, not Title
                Description = recipe.Description,
                ImageUrl = recipe.ImageUrl, // Model has ImageUrl, not Image
                Steps = recipe.Steps,
                CreatedAt = recipe.CreatedAt,
                UpdatedAt = recipe.UpdatedAt,
                Ingredients = recipe.RecipeIngredients.Select(ri => new RecipeIngredientDto
                {
                    IngredientId = ri.IngredientId,
                    Name = ri.Ingredient.Name,
                    Amount = ri.Amount,
                    Unit = ri.Unit
                }).ToList()
            }).ToList();
        }

        public async Task<RecipeResponseDto?> GetByIdAsync(Guid id)
        {
            var recipe = await _context.Recipes
                .Include(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Ingredient)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null)
                return null;

            // Direct mapping following the actual model properties
            return new RecipeResponseDto
            {
                Id = recipe.Id,
                Name = recipe.Name,
                Description = recipe.Description,
                ImageUrl = recipe.ImageUrl,
                Steps = recipe.Steps,
                CreatedAt = recipe.CreatedAt,
                UpdatedAt = recipe.UpdatedAt,
                Ingredients = recipe.RecipeIngredients.Select(ri => new RecipeIngredientDto
                {
                    IngredientId = ri.IngredientId,
                    Name = ri.Ingredient.Name,
                    Amount = ri.Amount,
                    Unit = ri.Unit
                }).ToList()
            };
        }

        public async Task<RecipeResponseDto> CreateAsync(RecipeCreateDto dto, Guid userId)
        {
            // Get the user
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new ArgumentException("User not found");

            // Get ingredients if provided
            var ingredients = new List<Ingredient>();
            if (dto.IngredientIds.Any())
            {
                ingredients = await _context.Ingredients
                    .Where(i => dto.IngredientIds.Contains(i.Id))
                    .ToListAsync();
            }

            // Create new recipe following the model structure
            var recipe = new Recipes
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                Steps = dto.Steps,
                CreatedAt = DateTime.UtcNow,
                RecipeIngredients = ingredients.Select(ingredient => new RecipeIngredient
                {
                    IngredientId = ingredient.Id,
                    Amount = 1,
                    Unit = "g",
                    Note = ""
                }).ToList(),
                SavedByUsers = new List<User> { user }
            };

            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();

            // Return mapped response
            return new RecipeResponseDto
            {
                Id = recipe.Id,
                Name = recipe.Name,
                Description = recipe.Description,
                ImageUrl = recipe.ImageUrl,
                Steps = recipe.Steps,
                CreatedAt = recipe.CreatedAt,
                UpdatedAt = recipe.UpdatedAt,
                Ingredients = recipe.RecipeIngredients.Select(ri => new RecipeIngredientDto
                {
                    IngredientId = ri.IngredientId,
                    Name = ri.Ingredient.Name,
                    Amount = ri.Amount,
                    Unit = ri.Unit
                }).ToList()
            };
        }

        public async Task<RecipeResponseDto?> UpdateAsync(Guid id, RecipeUpdateDto dto)
        {
            var recipe = await _context.Recipes
                .Include(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Ingredient)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null)
                return null;

            // Update properties following the model
            recipe.Name = dto.Name;
            recipe.Description = dto.Description;
            recipe.ImageUrl = dto.ImageUrl;
            recipe.Steps = dto.Steps;
            recipe.UpdatedAt = DateTime.UtcNow;

            // Update ingredients if provided
            if (dto.IngredientIds.Any())
            {
                var newIngredients = await _context.Ingredients
                    .Where(i => dto.IngredientIds.Contains(i.Id))
                    .ToListAsync();
                recipe.RecipeIngredients = newIngredients.Select(ingredient => new RecipeIngredient
                {
                    IngredientId = ingredient.Id,
                    Amount = 1,
                    Unit = "g",
                    Note = ""
                }).ToList();
            }

            await _context.SaveChangesAsync();

            return new RecipeResponseDto
            {
                Id = recipe.Id,
                Name = recipe.Name,
                Description = recipe.Description,
                ImageUrl = recipe.ImageUrl,
                Steps = recipe.Steps,
                CreatedAt = recipe.CreatedAt,
                UpdatedAt = recipe.UpdatedAt,
                Ingredients = recipe.RecipeIngredients.Select(ri => new RecipeIngredientDto
                {
                    IngredientId = ri.IngredientId,
                    Name = ri.Ingredient.Name,
                    Amount = ri.Amount,
                    Unit = ri.Unit
                }).ToList()
            };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null)
                return false;

            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<RecipeResponseDto>> GetUserSavedRecipesAsync(Guid userId)
        {
            var recipes = await _context.Recipes
                .Include(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Ingredient)
                .Where(r => r.SavedByUsers.Any(u => u.Id == userId))
                .ToListAsync();

            return recipes.Select(recipe => new RecipeResponseDto
            {
                Id = recipe.Id,
                Name = recipe.Name,
                Description = recipe.Description,
                ImageUrl = recipe.ImageUrl,
                Steps = recipe.Steps,
                CreatedAt = recipe.CreatedAt,
                UpdatedAt = recipe.UpdatedAt,
                Ingredients = recipe.RecipeIngredients.Select(ri => new RecipeIngredientDto
                {
                    IngredientId = ri.IngredientId,
                    Name = ri.Ingredient.Name,
                    Amount = ri.Amount,
                    Unit = ri.Unit
                }).ToList()
            }).ToList();
        }
    }
}