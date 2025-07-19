using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MealPlannerApp.DTOs
{
    public class GetRecipesDto
    {
        // Text search (e.g., "chicken curry")
        public string? Query { get; set; }

        // Max number of results to return
        public int Number { get; set; } = 10;

        // Optional: Filter by cuisine (e.g., "Italian", "Mexican")
        public string? Cuisine { get; set; }

        // Optional: Filter by diet (e.g., "vegetarian", "gluten free")
        public string? Diet { get; set; }

        // Comma-separated ingredients to exclude (e.g., "peanuts,shellfish")
        public string? ExcludeIngredients { get; set; }

        // Comma-separated ingredients to include (e.g., "tomato,onion,egg")
        public string? IncludeIngredients { get; set; }

        // Optional: Filter by meal type (e.g., "main course", "dessert")
        public string? Type { get; set; } = "main course";

    }

    public class RecipeResponseDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public string? ImageUrl { get; set; }
        public required string[] Steps { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class SpoonacularRecipeDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public string ImageType { get; set; } = string.Empty;
        public List<SpoonacularIngredientDto> UsedIngredients { get; set; } = new();
        public List<SpoonacularIngredientDto> MissedIngredients { get; set; } = new();
        public List<SpoonacularIngredientDto> UnusedIngredients { get; set; } = new();
    }

    public class SpoonacularIngredientDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public double Amount { get; set; }
        public string Unit { get; set; } = string.Empty;
    }
}