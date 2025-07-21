using MealPlannerApp.Config;
using MealPlannerApp.DTOs;
using System.Text;
using System.Text.Json;

namespace MealPlannerApp.Services
{
    public class SpoonacularService
    {
        private readonly ILogger<SpoonacularService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonOptions;

        public SpoonacularService(
            ILogger<SpoonacularService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
        }

        public async Task<List<GetRecipeResponseDto>> GetRecipes(GetRecipesDto dto)
        {
            try
            {
                ValidateRequest(dto);

                var apiUrl = BuildApiUrl(dto);
                _logger.LogInformation("Calling Spoonacular API: {Url}", HideApiKey(apiUrl));

                if (!string.IsNullOrWhiteSpace(dto.IncludeIngredients))
                {
                    // For findByIngredients endpoint, it returns List<GetRecipeResponseDto> directly
                    var ingredientRecipes = await CallSpoonacularApi<List<GetRecipeResponseDto>>(apiUrl);
                    _logger.LogInformation("Successfully processed {Count} recipes", ingredientRecipes?.Count ?? 0);
                    return ingredientRecipes ?? new List<GetRecipeResponseDto>();
                }
                else
                {
                    // For complexSearch endpoint, it returns a wrapper with Results property
                    var searchResponse = await CallSpoonacularApi<SpoonacularSearchResponse>(apiUrl);
                    _logger.LogInformation("Successfully processed {Count} recipes", searchResponse?.Results?.Count ?? 0);
                    return searchResponse?.Results ?? new List<GetRecipeResponseDto>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching recipes");
                throw;
            }
        }

        public async Task<RecipeDetailsResponseDto?> GetRecipeDetails(GetRecipeDetailsDto dto)
        {
            try
            {
                ValidateRecipeDetailsRequest(dto);

                var apiUrl = BuildRecipeDetailsApiUrl(dto);
                _logger.LogInformation("Calling Spoonacular Recipe Details API: {Url}", HideApiKey(apiUrl));

                var recipeDetails = await CallSpoonacularApi<RecipeDetailsResponseDto>(apiUrl);
                _logger.LogInformation("Successfully processed recipe details for recipe {Id}", dto.Id);

                return recipeDetails;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching recipe details for recipe {Id}", dto.Id);
                throw;
            }
        }

        private static void ValidateRequest(GetRecipesDto dto)
        {
            if (dto.Number <= 0 || dto.Number > 100)
                throw new ArgumentException("Number of results must be between 1 and 100");
        }

        private static void ValidateRecipeDetailsRequest(GetRecipeDetailsDto dto)
        {
            if (dto.Id <= 0)
                throw new ArgumentException("Recipe ID must be greater than 0");
        }

        private static string BuildRecipeDetailsApiUrl(GetRecipeDetailsDto dto)
        {
            if (string.IsNullOrWhiteSpace(AppConfig.SpoonacularApiKey))
                throw new InvalidOperationException("Spoonacular API key is not configured");

            return $"https://api.spoonacular.com/recipes/{dto.Id}/information?apiKey={AppConfig.SpoonacularApiKey}";
        }

        private static string BuildApiUrl(GetRecipesDto dto)
        {
            if (string.IsNullOrWhiteSpace(AppConfig.SpoonacularApiKey))
                throw new InvalidOperationException("Spoonacular API key is not configured");

            return !string.IsNullOrWhiteSpace(dto.IncludeIngredients)
                ? BuildFindByIngredientsUrl(dto)
                : BuildComplexSearchUrl(dto);
        }

        private static string BuildFindByIngredientsUrl(GetRecipesDto dto)
        {
            var query = new StringBuilder("https://api.spoonacular.com/recipes/findByIngredients?");

            query.Append($"ingredients={Uri.EscapeDataString(dto.IncludeIngredients!)}");
            query.Append($"&number={dto.Number}");
            query.Append("&ignorePantry=true");
            query.Append("&ranking=1");

            AppendOptionalParameter(query, "excludeIngredients", dto.ExcludeIngredients);
            query.Append($"&apiKey={AppConfig.SpoonacularApiKey}");

            return query.ToString();
        }

        private static string BuildComplexSearchUrl(GetRecipesDto dto)
        {
            var query = new StringBuilder("https://api.spoonacular.com/recipes/complexSearch?");

            AppendOptionalParameter(query, "query", dto.Query);
            AppendOptionalParameter(query, "cuisine", dto.Cuisine);
            AppendOptionalParameter(query, "diet", dto.Diet);
            AppendOptionalParameter(query, "excludeIngredients", dto.ExcludeIngredients);
            AppendOptionalParameter(query, "type", dto.Type);

            query.Append($"number={dto.Number}&");
            query.Append($"apiKey={AppConfig.SpoonacularApiKey}");

            return query.ToString();
        }

        private static void AppendOptionalParameter(StringBuilder query, string paramName, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                query.Append($"&{paramName}={Uri.EscapeDataString(value)}");
        }

        private async Task<T?> CallSpoonacularApi<T>(string url) where T : class
        {
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(30);

                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Spoonacular API returned {StatusCode}", response.StatusCode);
                    return null;
                }

                var jsonString = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(jsonString, _jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error when calling Spoonacular API");
                throw new Exception("Failed to fetch data from Spoonacular API", ex);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout when calling Spoonacular API");
                throw new Exception("Spoonacular API request timed out", ex);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error parsing JSON response from Spoonacular API");
                throw new Exception("Invalid response format from Spoonacular API", ex);
            }
        }

        private static string HideApiKey(string url) =>
            url.Replace(AppConfig.SpoonacularApiKey, "[API_KEY_HIDDEN]");
    }

    // Helper class for complex search response wrapper
    public class SpoonacularSearchResponse
    {
        public List<GetRecipeResponseDto> Results { get; set; } = new List<GetRecipeResponseDto>();
        public int Offset { get; set; }
        public int Number { get; set; }
        public int TotalResults { get; set; }
    }
}