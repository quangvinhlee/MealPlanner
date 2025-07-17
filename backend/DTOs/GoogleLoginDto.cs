namespace MealPlannerApp.DTOs
{
    public class GoogleLoginDto
    {
        public required string GoogleId { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public string? AvatarUrl { get; set; }
    }
}