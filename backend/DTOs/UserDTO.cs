using System;
using System.Collections.Generic;

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

    public class UserResponseDto
    {
        public Guid Id { get; set; }
        public required string GoogleId { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public string? AvatarUrl { get; set; }
        public required List<FridgeItemResponseDto> FridgeItems { get; set; }



    }
}