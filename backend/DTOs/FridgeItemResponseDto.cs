namespace MealPlannerApp.DTOs
{
    public class FridgeItemResponseDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string? Quantity { get; set; }
        public string? Unit { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }
}