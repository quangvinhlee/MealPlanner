namespace MealPlannerApp.DTOs
{
    public class UserLoginDto
    {
        /// <example>123456789</example>
        public required string GoogleId { get; set; }
        /// <example>vinh</example>
        public required string Name { get; set; }
        /// <example>lequangvinh224@gmail.com</example>
        public required string Email { get; set; }
        public string? AvatarUrl { get; set; }
    }
}