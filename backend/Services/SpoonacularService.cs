using MealPlannerApp.Config;
using MealPlannerApp.DTOs;
using AutoMapper;
using System.Text;
using Newtonsoft.Json.Linq;

namespace MealPlannerApp.Services
{
    public class SpoonacularService
    {
        private readonly IMapper _mapper;
        private readonly ILogger<SpoonacularService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public SpoonacularService(
            IMapper mapper,
            ILogger<SpoonacularService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _mapper = mapper;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<List<RecipeResponseDto>> GetRecipes(GetRecipesDto dto)
        {
            try
            {
                ValidateRequest(dto);

                var apiUrl = BuildApiUrl(dto);
                _logger.LogInformation("Calling Spoonacular API: {Url}", HideApiKey(apiUrl));

                var response = await CallSpoonacularApi(apiUrl);
                if (response == null)
                {
                    _logger.LogWarning("Spoonacular API returned null response");
                    return new List<RecipeResponseDto>();
                }

                var recipes = ProcessApiResponse(response, dto);
                _logger.LogInformation("Successfully processed {Count} recipes", recipes.Count);

                return recipes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching recipes");
                throw;
            }
        }

        private static void ValidateRequest(GetRecipesDto dto)
        {
            if (dto.Number <= 0 || dto.Number > 100)
                throw new ArgumentException("Number of results must be between 1 and 100");
        }

        private string BuildApiUrl(GetRecipesDto dto)
        {
            if (string.IsNullOrWhiteSpace(AppConfig.SpoonacularApiKey))
            {
                throw new InvalidOperationException("Spoonacular API key is not configured");
            }

            return !string.IsNullOrWhiteSpace(dto.IncludeIngredients)
                ? BuildFindByIngredientsUrl(dto)
                : BuildComplexSearchUrl(dto);
        }

        private List<RecipeResponseDto> ProcessApiResponse(JToken response, GetRecipesDto dto)
        {
            return !string.IsNullOrWhiteSpace(dto.IncludeIngredients)
                ? ProcessFindByIngredientsResponse(response)
                : ProcessComplexSearchResponse(response);
        }

        private List<RecipeResponseDto> ProcessFindByIngredientsResponse(JToken response)
        {
            if (response is not JArray jArray)
            {
                _logger.LogWarning("Expected JArray but got {Type}", response.Type);
                return new List<RecipeResponseDto>();
            }

            return MapRecipesFromArray(jArray);
        }

        private List<RecipeResponseDto> ProcessComplexSearchResponse(JToken response)
        {
            if (response is not JObject jObj || jObj["results"] is not JArray resultsArray)
            {
                _logger.LogWarning("Expected JObject with 'results' array but got {Type}", response?.GetType().Name);
                return new List<RecipeResponseDto>();
            }

            return MapRecipesFromArray(resultsArray);
        }

        private List<RecipeResponseDto> MapRecipesFromArray(JArray jArray) =>
            jArray
                .OfType<JObject>()
                .Select(jObj => _mapper.Map<RecipeResponseDto>(jObj))
                .Where(recipe => recipe != null)
                .ToList();

        private string BuildFindByIngredientsUrl(GetRecipesDto dto)
        {
            var query = new StringBuilder("https://api.spoonacular.com/recipes/findByIngredients?");

            query.Append($"ingredients={Uri.EscapeDataString(dto.IncludeIngredients!)}");
            query.Append($"&number={dto.Number}");
            query.Append("&ignorePantry=true");
            query.Append("&ranking=1");

            // Fixed: Add proper & before excludeIngredients
            AppendOptionalParameter(query, "excludeIngredients", dto.ExcludeIngredients);

            query.Append($"&apiKey={AppConfig.SpoonacularApiKey}");

            return query.ToString();
        }

        private string BuildComplexSearchUrl(GetRecipesDto dto)
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

        // Fixed: This method now properly adds & before parameter name
        private static void AppendOptionalParameter(StringBuilder query, string paramName, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                query.Append($"&{paramName}={Uri.EscapeDataString(value)}");
            }
        }

        private async Task<JToken?> CallSpoonacularApi(string url)
        {
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(30);

                var response = await httpClient.GetStringAsync(url);
                return JToken.Parse(response);
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing JSON response from Spoonacular API");
                throw new Exception("Invalid response format from Spoonacular API", ex);
            }
        }

        private static string HideApiKey(string url) =>
            url.Replace(AppConfig.SpoonacularApiKey, "[API_KEY_HIDDEN]");
    }
}