using System;
using System.Collections.Generic;

namespace MealPlannerApp.Models
{
    public class MealPlan
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? TargetCalories { get; set; }
        public string? Diet { get; set; }
        public string? Exclude { get; set; }
        public ICollection<MealPlanDay> Days { get; set; } = new List<MealPlanDay>();
    }
}