using Microsoft.Extensions.Configuration;

namespace MealPlannerApp.Config
{
    public static class AppConfig
    {
        public static string SpoonacularApiKey { get; private set; } = string.Empty;
        public static string JwtKey { get; private set; } = string.Empty;
        public static string JwtIssuer { get; private set; } = string.Empty;
        public static string JwtAudience { get; private set; } = string.Empty;
        public static string GoogleClientId { get; private set; } = string.Empty;
        public static string GoogleClientSecret { get; private set; } = string.Empty;

        public static void Initialize(IConfiguration configuration)
        {
            SpoonacularApiKey = configuration["Spoonacular:ApiKey"] ??
                throw new InvalidOperationException("Spoonacular:ApiKey is not configured");

            JwtKey = configuration["Jwt:Key"] ??
                throw new InvalidOperationException("Jwt:Key is not configured");

            JwtIssuer = configuration["Jwt:Issuer"] ?? "MealPlannerApp";
            JwtAudience = configuration["Jwt:Audience"] ?? "MealPlannerApp";

            GoogleClientId = configuration["Authentication:Google:ClientId"] ??
                throw new InvalidOperationException("Google ClientId is not configured");

            GoogleClientSecret = configuration["Authentication:Google:ClientSecret"] ??
                throw new InvalidOperationException("Google ClientSecret is not configured");
        }
    }
}