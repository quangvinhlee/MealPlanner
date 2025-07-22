using System;
using System.Collections.Generic;

namespace MealPlannerApp.DTOs
{
    public class UserLoginDto
    {
        /// <example>123456789</example>
        public required string GoogleId { get; set; }
        /// <example>vinh</example>
        public required string Name { get; set; }
        /// <example>lequangvinh224@gmail.com</example>
        public required string Email { get; set; }
        public string? AvatarUrl { get; set; }
    }

    public class UserResponseDto
    {
        public Guid Id { get; set; }
        public required string GoogleId { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public string? AvatarUrl { get; set; }
        public required List<FridgeItemResponseDto> FridgeItems { get; set; }
        public List<RecipeResponseDto> SavedRecipes { get; set; } = new();
        public List<UserMealPlanDto> MealPlans { get; set; } = new();
    }

    public class UserMealPlanDto
    {
        public Guid Id { get; set; }
        public int? TargetCalories { get; set; }
        public string? Diet { get; set; }
        public string? Exclude { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<UserMealPlanDayDto> Days { get; set; } = new();
    }

    public class UserMealPlanDayDto
    {
        public string DayOfWeek { get; set; } = string.Empty;
        public double Calories { get; set; }
        public double Protein { get; set; }
        public double Fat { get; set; }
        public double Carbohydrates { get; set; }
        public List<UserMealPlanMealDto> Meals { get; set; } = new();
    }

    public class UserMealPlanMealDto
    {
        public int SpoonacularId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Image { get; set; }
        public string? ImageType { get; set; }
        public string? SourceUrl { get; set; }
        public int ReadyInMinutes { get; set; }
        public int Servings { get; set; }
    }
}