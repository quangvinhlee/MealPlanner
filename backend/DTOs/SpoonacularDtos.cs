using System.ComponentModel.DataAnnotations;

namespace MealPlannerApp.DTOs
{
    public class GetRecipesDto
    {
        /// <summary>
        /// General search query
        /// </summary>
        /// <example>pasta</example>
        public string? Query { get; set; }

        /// <summary>
        /// Max number of results to return
        /// </summary>
        /// <example>10</example>
        [System.ComponentModel.DataAnnotations.Range(1, 100)]
        public int Number { get; set; } = 10;

        /// <summary>
        /// Optional: Filter by cuisine (e.g., "Italian", "Mexican")
        /// </summary>
        /// <example>Italian</example>
        public string? Cuisine { get; set; }

        /// <summary>
        /// Optional: Filter by diet (e.g., "vegetarian", "gluten free")
        /// </summary>
        /// <example>vegetarian</example>
        public string? Diet { get; set; }

        /// <summary>
        /// Comma-separated ingredients to exclude (e.g., "peanuts,shellfish")
        /// </summary>
        /// <example>peanuts,shellfish</example>
        public string? ExcludeIngredients { get; set; }

        /// <summary>
        /// Comma-separated ingredients to include (e.g., "tomato,onion,egg")
        /// </summary>
        /// <example>tomato,onion,egg</example>
        public string? IncludeIngredients { get; set; }

        /// <summary>
        /// Optional: Filter by meal type (e.g., "main course", "dessert")
        /// </summary>
        /// <example>main course</example>
        public string? Type { get; set; } = "main course";
    }

    public class RecipeResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Image { get; set; }
        public int UsedIngredientCount { get; set; }
        public int MissedIngredientCount { get; set; }
        public List<SpoonacularIngredientDto> UsedIngredients { get; set; } = new();
        public List<SpoonacularIngredientDto> MissedIngredients { get; set; } = new();
        public List<SpoonacularIngredientDto> UnusedIngredients { get; set; } = new();
        public int Likes { get; set; }
    }

    public class SpoonacularIngredientDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Original { get; set; } = string.Empty;
        public string? Image { get; set; }
        public double Amount { get; set; }
        public string Unit { get; set; } = string.Empty;
    }
}