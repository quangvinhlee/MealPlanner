

namespace MealPlannerApp.Models
{
    public class User
    {
        public Guid Id { get; set; }

        public required string GoogleId { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }

        public string? AvatarUrl { get; set; }

        public required ICollection<FridgeItems> FridgeItems { get; set; } = new List<FridgeItems>();
        public required ICollection<Recipes> SavedRecipes { get; set; } = new List<Recipes>();

        public required ICollection<ShoppingListItems> ShoppingListItems { get; set; } = new List<ShoppingListItems>();
    }
}