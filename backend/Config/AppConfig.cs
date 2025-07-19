using Microsoft.Extensions.Configuration;

namespace MealPlannerApp.Config
{
    public static class AppConfig
    {
        private static IConfiguration? _configuration;

        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // JWT Configuration
        public static string JwtKey => _configuration?["Jwt:Key"] ?? throw new Exception("JWT Key not configured");
        public static string JwtIssuer => _configuration?["Jwt:Issuer"] ?? throw new Exception("JWT Issuer not configured");
        public static string JwtAudience => _configuration?["Jwt:Audience"] ?? throw new Exception("JWT Audience not configured");

        // Google Authentication
        public static string GoogleClientId => _configuration?["Authentication:Google:ClientId"] ?? throw new Exception("Google ClientId not configured");
        public static string GoogleClientSecret => _configuration?["Authentication:Google:ClientSecret"] ?? throw new Exception("Google ClientSecret not configured");

        // Database
        public static string DefaultConnection => _configuration?["ConnectionStrings:DefaultConnection"] ?? throw new Exception("DefaultConnection not configured");

        //Spoonacular
        public static string SpoonacularApiKey => _configuration?["Spoonacular:ApiKey"] ?? throw new Exception("SpoonacularApiKey not configured");
    }
}