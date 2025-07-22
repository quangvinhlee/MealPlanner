using MealPlannerApp.Models;
using MealPlannerApp.DTOs;
using System;
using System.Threading.Tasks;
using System.Linq;
using MealPlannerApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace MealPlannerApp.Services
{
    public class MealPlanService
    {
        private readonly AppDbContext _context;

        public MealPlanService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<MealPlan> SaveMealPlanAsync(Guid userId, MealPlanCreateDto dto)
        {
            var mealPlan = new MealPlan
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TargetCalories = dto.TargetCalories,
                Diet = dto.Diet,
                Exclude = dto.Exclude,
                CreatedAt = DateTime.UtcNow,
                Days = dto.Week.Select(kvp => new MealPlanDay
                {
                    DayOfWeek = kvp.Key,
                    Calories = kvp.Value.Calories,
                    Protein = kvp.Value.Protein,
                    Fat = kvp.Value.Fat,
                    Carbohydrates = kvp.Value.Carbohydrates,
                    Meals = kvp.Value.Meals.Select(m => new MealPlanMeal
                    {
                        SpoonacularId = m.SpoonacularId,
                        Title = m.Title,
                        Image = m.Image,
                        ImageType = m.ImageType,
                        SourceUrl = m.SourceUrl,
                        ReadyInMinutes = m.ReadyInMinutes,
                        Servings = m.Servings
                    }).ToList()
                }).ToList()
            };

            _context.MealPlans.Add(mealPlan);
            await _context.SaveChangesAsync();
            return mealPlan;
        }

        public async Task<List<MealPlan>> GetMealPlansForUserAsync(Guid userId)
        {
            return await _context.MealPlans
                .Include(mp => mp.Days)
                .ThenInclude(d => d.Meals)
                .Where(mp => mp.UserId == userId)
                .ToListAsync();
        }

        public async Task<bool> DeleteMealPlanAsync(Guid userId, Guid mealPlanId)
        {
            var mealPlan = await _context.MealPlans
                .Where(mp => mp.UserId == userId && mp.Id == mealPlanId)
                .FirstOrDefaultAsync();
            if (mealPlan == null)
                return false;
            _context.MealPlans.Remove(mealPlan);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}