using MealPlannerApp.Models;
using MealPlannerApp.Data;
using Microsoft.EntityFrameworkCore;

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

        public async Task<Ingredient?> GetByIdAsync(Guid id)
        {
            return await _context.Ingredients.FindAsync(id);
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
            existing.Quantity = ingredient.Quantity;
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