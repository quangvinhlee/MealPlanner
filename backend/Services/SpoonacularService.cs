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

        public async Task<RecipeListWithNextHrefDto> GetRecipes(GetRecipesDto dto)
        {
            try
            {
                ValidateRequest(dto);

                // Use NextHref if provided, otherwise build the URL
                var apiUrl = !string.IsNullOrWhiteSpace(dto.NextHref)
                    ? dto.NextHref
                    : BuildApiUrl(dto);

                _logger.LogInformation("Calling Spoonacular API: {Url}", HideApiKey(apiUrl));

                if (!string.IsNullOrWhiteSpace(dto.IncludeIngredients))
                {
                    var ingredientRecipes = await CallSpoonacularApi<List<GetRecipeResponseDto>>(apiUrl);
                    var nextHref = CalculateNextHrefForIngredients(dto, ingredientRecipes?.Count ?? 0);
                    return new RecipeListWithNextHrefDto
                    {
                        Results = ingredientRecipes ?? new List<GetRecipeResponseDto>(),
                        NextHref = nextHref
                    };
                }
                else
                {
                    var searchResponse = await CallSpoonacularApi<SpoonacularSearchResponse>(apiUrl);
                    var nextHref = CalculateNextHrefForComplexSearch(dto, searchResponse);
                    return new RecipeListWithNextHrefDto
                    {
                        Results = searchResponse?.Results ?? new List<GetRecipeResponseDto>(),
                        NextHref = nextHref
                    };
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

        public async Task<SearchRecipesResponseDto> SearchRecipes(SearchRecipesRequestDto dto)
        {
            try
            {
                // Build the API URL for complexSearch
                var apiUrl = BuildComplexSearchUrl(dto);
                _logger.LogInformation("Calling Spoonacular API (complexSearch): {Url}", HideApiKey(apiUrl));

                var searchResponse = await CallSpoonacularApi<SpoonacularSearchResponse>(apiUrl);
                if (searchResponse == null)
                {
                    return new SearchRecipesResponseDto
                    {
                        Results = new List<GetRecipeResponseDto>(),
                        Offset = dto.Offset,
                        Number = dto.Number,
                        TotalResults = 0,
                        NextHref = null
                    };
                }

                // Calculate nextHref for pagination
                int nextOffset = dto.Offset + searchResponse.Number;
                string? nextHref = null;
                if (nextOffset < searchResponse.TotalResults)
                {
                    var query = new StringBuilder($"/api/spoonacular/recipes/complexSearch?");
                    if (!string.IsNullOrWhiteSpace(dto.Query)) query.Append($"query={Uri.EscapeDataString(dto.Query)}&");
                    if (!string.IsNullOrWhiteSpace(dto.Cuisine)) query.Append($"cuisine={Uri.EscapeDataString(dto.Cuisine)}&");
                    if (!string.IsNullOrWhiteSpace(dto.Diet)) query.Append($"diet={Uri.EscapeDataString(dto.Diet)}&");
                    if (!string.IsNullOrWhiteSpace(dto.ExcludeIngredients)) query.Append($"excludeIngredients={Uri.EscapeDataString(dto.ExcludeIngredients)}&");
                    if (!string.IsNullOrWhiteSpace(dto.Type)) query.Append($"type={Uri.EscapeDataString(dto.Type)}&");
                    query.Append($"number={dto.Number}&offset={nextOffset}");
                    nextHref = query.ToString();
                }

                return new SearchRecipesResponseDto
                {
                    Results = searchResponse.Results?.Select(r => new GetRecipeResponseDto
                    {
                        Id = r.Id,
                        Title = r.Title,
                        Image = r.Image,
                        UsedIngredientCount = r.UsedIngredientCount,
                        MissedIngredientCount = r.MissedIngredientCount,
                        UsedIngredients = r.UsedIngredients,
                        MissedIngredients = r.MissedIngredients,
                        UnusedIngredients = r.UnusedIngredients,
                        Likes = r.Likes
                    }).ToList() ?? new List<GetRecipeResponseDto>(),
                    Offset = searchResponse.Offset,
                    Number = searchResponse.Number,
                    TotalResults = searchResponse.TotalResults,
                    NextHref = nextHref
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching recipes (complexSearch)");
                throw;
            }
        }

        public async Task<GenerateMealPlanResponseDto?> GenerateMealPlan(GenerateMealPlanRequestDto dto)
        {
            try
            {
                var apiUrl = BuildMealPlanUrl(dto);
                _logger.LogInformation("Calling Spoonacular API (mealplanner/generate): {Url}", HideApiKey(apiUrl));
                var response = await CallSpoonacularApi<GenerateMealPlanResponseDto>(apiUrl);

                var totalNutrients = new NutrientsDto();

                if (response?.Week != null)
                {
                    foreach (var day in response.Week.Values)
                    {
                        if (day.Nutrients != null)
                        {
                            totalNutrients.Calories += day.Nutrients.Calories;
                            totalNutrients.Protein += day.Nutrients.Protein;
                            totalNutrients.Fat += day.Nutrients.Fat;
                            totalNutrients.Carbohydrates += day.Nutrients.Carbohydrates;
                        }
                    }
                }

                // Assign the calculated total to the response
                response!.Nutrients = totalNutrients;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating meal plan");
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

        // Overload for building complexSearch URL from SearchRecipesRequestDto
        private static string BuildComplexSearchUrl(SearchRecipesRequestDto dto)
        {
            var query = new StringBuilder("https://api.spoonacular.com/recipes/complexSearch?");
            if (!string.IsNullOrWhiteSpace(dto.Query)) query.Append($"query={Uri.EscapeDataString(dto.Query)}&");
            if (!string.IsNullOrWhiteSpace(dto.Cuisine)) query.Append($"cuisine={Uri.EscapeDataString(dto.Cuisine)}&");
            if (!string.IsNullOrWhiteSpace(dto.Diet)) query.Append($"diet={Uri.EscapeDataString(dto.Diet)}&");
            if (!string.IsNullOrWhiteSpace(dto.ExcludeIngredients)) query.Append($"excludeIngredients={Uri.EscapeDataString(dto.ExcludeIngredients)}&");
            if (!string.IsNullOrWhiteSpace(dto.Type)) query.Append($"type={Uri.EscapeDataString(dto.Type)}&");
            query.Append($"number={dto.Number}&offset={dto.Offset}&apiKey={AppConfig.SpoonacularApiKey}");
            return query.ToString();
        }

        private static string BuildMealPlanUrl(GenerateMealPlanRequestDto dto)
        {
            var query = new StringBuilder("https://api.spoonacular.com/mealplanner/generate?");
            if (dto.TargetCalories.HasValue) query.Append($"targetCalories={dto.TargetCalories.Value}&");
            if (!string.IsNullOrWhiteSpace(dto.Diet)) query.Append($"diet={Uri.EscapeDataString(dto.Diet)}&");
            if (!string.IsNullOrWhiteSpace(dto.Exclude)) query.Append($"exclude={Uri.EscapeDataString(dto.Exclude)}&");
            // Default to week plan if not specified
            query.Append("timeFrame=week&");
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
            string? jsonString = null;
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

                jsonString = await response.Content.ReadAsStringAsync();
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
                _logger.LogError("Error parsing JSON response from Spoonacular API. Raw response: {Json}", jsonString);
                throw new Exception("Invalid response format from Spoonacular API", ex);
            }
        }

        private static string HideApiKey(string url) =>
            url.Replace(AppConfig.SpoonacularApiKey, "[API_KEY_HIDDEN]");

        private static string? CalculateNextHrefForComplexSearch(GetRecipesDto dto, SpoonacularSearchResponse? searchResponse)
        {
            if (searchResponse == null || searchResponse.TotalResults == 0)
                return null;

            int nextOffset = dto.Offset + searchResponse.Number;
            if (nextOffset >= searchResponse.TotalResults)
                return null;

            var query = new StringBuilder("/api/spoonacular/recipes/complexSearch?");
            if (!string.IsNullOrWhiteSpace(dto.Query)) query.Append($"query={Uri.EscapeDataString(dto.Query)}&");
            if (!string.IsNullOrWhiteSpace(dto.Cuisine)) query.Append($"cuisine={Uri.EscapeDataString(dto.Cuisine)}&");
            if (!string.IsNullOrWhiteSpace(dto.Diet)) query.Append($"diet={Uri.EscapeDataString(dto.Diet)}&");
            if (!string.IsNullOrWhiteSpace(dto.ExcludeIngredients)) query.Append($"excludeIngredients={Uri.EscapeDataString(dto.ExcludeIngredients)}&");
            if (!string.IsNullOrWhiteSpace(dto.Type)) query.Append($"type={Uri.EscapeDataString(dto.Type)}&");
            query.Append($"number={dto.Number}&offset={nextOffset}");

            return query.ToString();
        }

        private static string? CalculateNextHrefForIngredients(GetRecipesDto dto, int currentCount)
        {
            if (currentCount < dto.Number)
                return null; // No more results

            int nextOffset = dto.Offset + dto.Number;
            var query = new StringBuilder("/api/spoonacular/recipes/findByIngredients?");
            query.Append($"ingredients={Uri.EscapeDataString(dto.IncludeIngredients!)}&number={dto.Number}&offset={nextOffset}&ignorePantry=true&ranking=1");
            if (!string.IsNullOrWhiteSpace(dto.ExcludeIngredients))
                query.Append($"&excludeIngredients={Uri.EscapeDataString(dto.ExcludeIngredients)}");
            // Do NOT append &apiKey

            return query.ToString();
        }
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