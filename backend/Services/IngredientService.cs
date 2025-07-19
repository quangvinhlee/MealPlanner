using MealPlannerApp.Models;
using MealPlannerApp.Data;
using Microsoft.EntityFrameworkCore;
using MealPlannerApp.DTOs;
using AutoMapper;

namespace MealPlannerApp.Services
{
    public class IngredientService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public IngredientService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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

            return _mapper.Map<IngredientDto>(ingredient);
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