namespace MealPlannerApp.DTOs
{
    public class FridgeItemCreateDto
    {
        public required string Quantity { get; set; }
        public required string Unit { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public required string IngredientName { get; set; }
    }

    public class FridgeItemUpdateDto
    {
        public required Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Quantity { get; set; }
        public required string Unit { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }

    public class FridgeItemResponseDto
    {
        public required Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Quantity { get; set; }
        public required string Unit { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }
}