namespace MealPlannerApp.Models
{
    public class RecipeIngredient
    {
        public Guid RecipeId { get; set; }
        public Recipes Recipe { get; set; } = null!;

        public Guid IngredientId { get; set; }
        public Ingredient Ingredient { get; set; } = null!;

        public double Amount { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string? Note { get; set; }
    }
}