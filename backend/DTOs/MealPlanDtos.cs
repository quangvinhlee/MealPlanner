using System;
using System.Collections.Generic;

namespace MealPlannerApp.DTOs
{
    public class MealPlanCreateDto
    {
        public int? TargetCalories { get; set; }
        public string? Diet { get; set; }
        public string? Exclude { get; set; }
        public Dictionary<string, MealPlanDayDto> Week { get; set; } = new();
    }



    public class MealPlanMealDto
    {
        public int SpoonacularId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Image { get; set; }
        public string? ImageType { get; set; }
        public string? SourceUrl { get; set; }
        public int ReadyInMinutes { get; set; }
        public int Servings { get; set; }
    }

    public class MealPlanDayDto
    {
        public List<MealPlanMealDto> Meals { get; set; } = new();
        public double Calories { get; set; }
        public double Protein { get; set; }
        public double Fat { get; set; }
        public double Carbohydrates { get; set; }
    }
}