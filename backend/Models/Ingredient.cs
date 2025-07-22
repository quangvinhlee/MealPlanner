namespace MealPlannerApp.Models
{
    public class Ingredient
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }

        public required ICollection<FridgeItems> FridgeItems { get; set; } = new List<FridgeItems>();
        public ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
    }
}