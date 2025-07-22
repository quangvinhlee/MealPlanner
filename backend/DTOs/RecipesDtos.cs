using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MealPlannerApp.DTOs
{
    public class RecipeCreateDto
    {
        /// <summary>
        /// Name of the recipe
        /// </summary>
        /// <example>Chocolate Chip Cookies</example>
        [Required]
        public required string Name { get; set; }

        /// <summary>
        /// Description of the recipe
        /// </summary>
        /// <example>Delicious homemade chocolate chip cookies</example>
        [Required]
        public required string Description { get; set; }

        /// <summary>
        /// Optional image URL
        /// </summary>
        /// <example>https://example.com/cookie.jpg</example>
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Array of cooking steps
        /// </summary>
        /// <example>["Preheat oven to 350°F", "Mix ingredients", "Bake for 12 minutes"]</example>
        [Required]
        public required string[] Steps { get; set; } = Array.Empty<string>();

        /// <summary>
        /// List of ingredient IDs used in this recipe
        /// </summary>
        public List<Guid> IngredientIds { get; set; } = new List<Guid>();
    }

    public class RecipeUpdateDto
    {
        /// <summary>
        /// Name of the recipe
        /// </summary>
        /// <example>Chocolate Chip Cookies</example>
        [Required]
        public required string Name { get; set; }

        /// <summary>
        /// Description of the recipe
        /// </summary>
        /// <example>Delicious homemade chocolate chip cookies</example>
        [Required]
        public required string Description { get; set; }

        /// <summary>
        /// Optional image URL
        /// </summary>
        /// <example>https://example.com/cookie.jpg</example>
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Array of cooking steps
        /// </summary>
        /// <example>["Preheat oven to 350°F", "Mix ingredients", "Bake for 12 minutes"]</example>
        [Required]
        public required string[] Steps { get; set; } = Array.Empty<string>();

        /// <summary>
        /// List of ingredient IDs used in this recipe
        /// </summary>
        public List<Guid> IngredientIds { get; set; } = new List<Guid>();
    }

    public class RecipeIngredientDto
    {
        public Guid IngredientId { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Amount { get; set; }
        public string Unit { get; set; } = string.Empty;
        // Optionally: public string? Note { get; set; }
    }

    public class RecipeResponseDto
    {
        /// <summary>
        /// Unique identifier of the recipe
        /// </summary>
        /// <example>550e8400-e29b-41d4-a716-446655440000</example>
        public required Guid Id { get; set; }

        /// <summary>
        /// Name of the recipe
        /// </summary>
        /// <example>Chocolate Chip Cookies</example>
        public required string Name { get; set; }

        /// <summary>
        /// Description of the recipe
        /// </summary>
        /// <example>Delicious homemade chocolate chip cookies</example>
        public required string Description { get; set; }

        /// <summary>
        /// Image URL if available
        /// </summary>
        /// <example>https://example.com/cookie.jpg</example>
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Array of cooking steps
        /// </summary>
        /// <example>["Preheat oven to 350°F", "Mix ingredients", "Bake for 12 minutes"]</example>
        public required string[] Steps { get; set; } = Array.Empty<string>();

        /// <summary>
        /// When the recipe was created
        /// </summary>
        /// <example>2024-01-15T10:30:00Z</example>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// When the recipe was last updated
        /// </summary>
        /// <example>2024-01-20T14:45:00Z</example>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// List of ingredients used in this recipe
        /// </summary>
        public List<RecipeIngredientDto> Ingredients { get; set; } = new List<RecipeIngredientDto>();
    }
}