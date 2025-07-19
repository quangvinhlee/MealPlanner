namespace MealPlannerApp.Models
{
    public class Recipes
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public string? ImageUrl { get; set; }
        public required string[] Steps { get; set; } = Array.Empty<string>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public required ICollection<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
        public required ICollection<User> SavedByUsers { get; set; } = new List<User>();
    }
}