using MealPlannerApp.Models;
using MealPlannerApp.Data;
using Microsoft.EntityFrameworkCore;
using MealPlannerApp.DTOs;

namespace MealPlannerApp.Services
{
    public class IngredientService
    {
        private readonly AppDbContext _context;
        public IngredientService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Ingredient>> GetAllAsync()
        {
            return await _context.Ingredients.ToListAsync();
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
            Console.WriteLine($"Ingredient: {ingredient.Name} (ID: {ingredient.Id})");
            Console.WriteLine($"FridgeItems count: {ingredient.FridgeItems?.Count ?? 0}");
            Console.WriteLine($"Recipes count: {ingredient.Recipes?.Count ?? 0}");

            return new IngredientDto
            {
                Id = ingredient.Id,
                Name = ingredient.Name,
                FridgeItems = (ingredient.FridgeItems ?? new List<FridgeItems>()).Select(fi => new FridgeItemResponseDto
                {
                    Id = fi.Id,
                    Name = fi.Name,
                    Quantity = fi.Quantity,
                    Unit = fi.Unit,
                    ExpirationDate = fi.ExpirationDate
                }).ToList(),
                Recipes = (ingredient.Recipes ?? new List<Recipes>()).Select(r => new RecipeResponseDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    ImageUrl = r.ImageUrl
                }).ToList()
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