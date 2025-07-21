using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MealPlannerApp.DTOs
{
    // Get recipes DTO
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

    public class GetRecipeResponseDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("image")]
        public string? Image { get; set; }

        [JsonPropertyName("usedIngredientCount")]
        public int UsedIngredientCount { get; set; }

        [JsonPropertyName("missedIngredientCount")]
        public int MissedIngredientCount { get; set; }

        [JsonPropertyName("usedIngredients")]
        public List<SpoonacularIngredientDto> UsedIngredients { get; set; } = new();

        [JsonPropertyName("missedIngredients")]
        public List<SpoonacularIngredientDto> MissedIngredients { get; set; } = new();

        [JsonPropertyName("unusedIngredients")]
        public List<SpoonacularIngredientDto> UnusedIngredients { get; set; } = new();

        [JsonPropertyName("likes")]
        public int Likes { get; set; }
    }

    public class SpoonacularIngredientDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("original")]
        public string Original { get; set; } = string.Empty;

        [JsonPropertyName("image")]
        public string? Image { get; set; }

        [JsonPropertyName("amount")]
        public double Amount { get; set; }

        [JsonPropertyName("unit")]
        public string Unit { get; set; } = string.Empty;
    }

    // Search response wrapper for complex search
    public class SpoonacularSearchResponse
    {
        [JsonPropertyName("results")]
        public List<RecipeResponseDto> Results { get; set; } = new();

        [JsonPropertyName("offset")]
        public int Offset { get; set; }

        [JsonPropertyName("number")]
        public int Number { get; set; }

        [JsonPropertyName("totalResults")]
        public int TotalResults { get; set; }
    }
}

// Get Recipe Details DTOs
public class GetRecipeDetailsDto
{
    /// <summary>
    /// The ID of the recipe to retrieve details for
    /// </summary>
    /// <example>12345</example>
    [Required]
    public int Id { get; set; }
}

public class RecipeDetailsResponseDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("image")]
    public string? Image { get; set; }

    [JsonPropertyName("imageType")]
    public string? ImageType { get; set; }

    [JsonPropertyName("instructions")]
    public string Instructions { get; set; } = string.Empty;

    [JsonPropertyName("extendedIngredients")]
    public List<ExtendedIngredientDto> ExtendedIngredients { get; set; } = new();

    [JsonPropertyName("readyInMinutes")]
    public int ReadyInMinutes { get; set; }

    [JsonPropertyName("servings")]
    public int Servings { get; set; }

    [JsonPropertyName("sourceUrl")]
    public string? SourceUrl { get; set; }

    [JsonPropertyName("vegetarian")]
    public bool Vegetarian { get; set; }

    [JsonPropertyName("vegan")]
    public bool Vegan { get; set; }

    [JsonPropertyName("glutenFree")]
    public bool GlutenFree { get; set; }

    [JsonPropertyName("dairyFree")]
    public bool DairyFree { get; set; }

    [JsonPropertyName("veryHealthy")]
    public bool VeryHealthy { get; set; }

    [JsonPropertyName("cheap")]
    public bool Cheap { get; set; }

    [JsonPropertyName("veryPopular")]
    public bool VeryPopular { get; set; }

    [JsonPropertyName("sustainable")]
    public bool Sustainable { get; set; }

    [JsonPropertyName("lowFodmap")]
    public bool LowFodmap { get; set; }

    [JsonPropertyName("weightWatcherSmartPoints")]
    public int WeightWatcherSmartPoints { get; set; }

    [JsonPropertyName("gaps")]
    public string? Gaps { get; set; }

    [JsonPropertyName("preparationMinutes")]
    public int? PreparationMinutes { get; set; }

    [JsonPropertyName("cookingMinutes")]
    public int? CookingMinutes { get; set; }

    [JsonPropertyName("aggregateLikes")]
    public int AggregateLikes { get; set; }

    [JsonPropertyName("healthScore")]
    public int HealthScore { get; set; }

    [JsonPropertyName("creditsText")]
    public string? CreditsText { get; set; }

    [JsonPropertyName("license")]
    public string? License { get; set; }

    [JsonPropertyName("sourceName")]
    public string? SourceName { get; set; }

    [JsonPropertyName("pricePerServing")]
    public double PricePerServing { get; set; }

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonPropertyName("cuisines")]
    public List<string> Cuisines { get; set; } = new();

    [JsonPropertyName("dishTypes")]
    public List<string> DishTypes { get; set; } = new();

    [JsonPropertyName("diets")]
    public List<string> Diets { get; set; } = new();

    [JsonPropertyName("occasions")]
    public List<string> Occasions { get; set; } = new();

    [JsonPropertyName("analyzedInstructions")]
    public List<AnalyzedInstructionDto> AnalyzedInstructions { get; set; } = new();

    [JsonPropertyName("spoonacularScore")]
    public double SpoonacularScore { get; set; }

    [JsonPropertyName("spoonacularSourceUrl")]
    public string? SpoonacularSourceUrl { get; set; }
}

public class ExtendedIngredientDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("aisle")]
    public string? Aisle { get; set; }

    [JsonPropertyName("image")]
    public string? Image { get; set; }

    [JsonPropertyName("consistency")]
    public string? Consistency { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("nameClean")]
    public string? NameClean { get; set; }

    [JsonPropertyName("original")]
    public string Original { get; set; } = string.Empty;

    [JsonPropertyName("originalName")]
    public string OriginalName { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public double Amount { get; set; }

    [JsonPropertyName("unit")]
    public string Unit { get; set; } = string.Empty;

    [JsonPropertyName("meta")]
    public List<string> Meta { get; set; } = new();

    [JsonPropertyName("measures")]
    public MeasuresDto Measures { get; set; } = new();
}

public class MeasuresDto
{
    [JsonPropertyName("us")]
    public MeasureDto Us { get; set; } = new();

    [JsonPropertyName("metric")]
    public MeasureDto Metric { get; set; } = new();
}

public class MeasureDto
{
    [JsonPropertyName("amount")]
    public double Amount { get; set; }

    [JsonPropertyName("unitShort")]
    public string UnitShort { get; set; } = string.Empty;

    [JsonPropertyName("unitLong")]
    public string UnitLong { get; set; } = string.Empty;
}

public class AnalyzedInstructionDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("steps")]
    public List<InstructionStepDto> Steps { get; set; } = new();
}

public class InstructionStepDto
{
    [JsonPropertyName("number")]
    public int Number { get; set; }

    [JsonPropertyName("step")]
    public string Step { get; set; } = string.Empty;

    [JsonPropertyName("ingredients")]
    public List<IngredientReferenceDto> Ingredients { get; set; } = new();

    [JsonPropertyName("equipment")]
    public List<EquipmentReferenceDto> Equipment { get; set; } = new();

    [JsonPropertyName("length")]
    public LengthDto? Length { get; set; }
}

public class IngredientReferenceDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("localizedName")]
    public string? LocalizedName { get; set; }

    [JsonPropertyName("image")]
    public string? Image { get; set; }
}

public class EquipmentReferenceDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("localizedName")]
    public string? LocalizedName { get; set; }

    [JsonPropertyName("image")]
    public string? Image { get; set; }
}

public class LengthDto
{
    [JsonPropertyName("number")]
    public int Number { get; set; }

    [JsonPropertyName("unit")]
    public string Unit { get; set; } = string.Empty;
}