namespace MealPlannerApp.Models
{
    public class Ingredient
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string? Quantity { get; set; }
    }
}