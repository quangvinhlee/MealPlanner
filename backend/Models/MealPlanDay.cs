using System;
using System.Collections.Generic;

namespace MealPlannerApp.Models
{
    public class MealPlanDay
    {
        public Guid Id { get; set; }
        public Guid MealPlanId { get; set; }
        public MealPlan MealPlan { get; set; } = null!;
        public string DayOfWeek { get; set; } = string.Empty;
        public ICollection<MealPlanMeal> Meals { get; set; } = new List<MealPlanMeal>();
        public double Calories { get; set; }
        public double Protein { get; set; }
        public double Fat { get; set; }
        public double Carbohydrates { get; set; }
    }
}