using System;

namespace MealPlannerApp.Models
{
    public class MealPlanMeal
    {
        public Guid Id { get; set; }
        public Guid MealPlanDayId { get; set; }
        public MealPlanDay MealPlanDay { get; set; } = null!;
        public int SpoonacularId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Image { get; set; }
        public string? ImageType { get; set; }
        public string? SourceUrl { get; set; }
        public int ReadyInMinutes { get; set; }
        public int Servings { get; set; }
    }
}