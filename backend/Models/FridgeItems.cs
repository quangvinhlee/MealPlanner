namespace MealPlannerApp.Models
{
    public class FridgeItems
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Quantity { get; set; }
        public required string Unit { get; set; }
        public DateTime? ExpirationDate { get; set; }

        public Guid IngredientId { get; set; }
        public Ingredient? Ingredient { get; set; }


        public Guid UserId { get; set; }
        public required User User { get; set; }
    }
}