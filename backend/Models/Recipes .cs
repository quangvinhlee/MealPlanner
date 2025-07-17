namespace MealPlannerApp.Models
{
    public class Recipes
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string ImageUrl { get; set; }
        public required string[] steps { get; set; }

        public required ICollection<Ingredient> Ingredients { get; set; } = new List<Ingredient>();

        public required ICollection<User> SavedByUsers { get; set; } = new List<User>();


    }
}