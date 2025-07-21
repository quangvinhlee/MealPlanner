using MealPlannerApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using MealPlannerApp.Config;

var builder = WebApplication.CreateBuilder(args);

// Enhanced logging: Add simple console logging with scopes and timestamps
builder.Logging.AddSimpleConsole(options =>
{
    options.IncludeScopes = true;
    options.SingleLine = true;
    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
});

// Initialize AppConfig
AppConfig.Initialize(builder.Configuration);

// Debug: Test if configuration is working
Console.WriteLine($"JWT Key configured: {!string.IsNullOrEmpty(builder.Configuration["Jwt:Key"])}");
Console.WriteLine($"Spoonacular Key configured: {!string.IsNullOrEmpty(builder.Configuration["Spoonacular:ApiKey"])}");

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MealPlannerApp API",
        Version = "v1"
    });

    // Include XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register services with correct names
builder.Services.AddScoped<MealPlannerApp.Services.IngredientService>();
builder.Services.AddScoped<MealPlannerApp.Services.UserService>();
builder.Services.AddScoped<MealPlannerApp.Services.FridgeItemService>();
builder.Services.AddScoped<MealPlannerApp.Services.SpoonacularService>();
builder.Services.AddScoped<MealPlannerApp.Services.RecipeService>();
builder.Services.AddHttpClient();

// Set Google as the default authentication scheme
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddGoogle(options =>
{
    options.ClientId = AppConfig.GoogleClientId;
    options.ClientSecret = AppConfig.GoogleClientSecret;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = AppConfig.JwtIssuer,
        ValidAudience = AppConfig.JwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AppConfig.JwtKey)),
        ClockSkew = TimeSpan.Zero // Remove clock skew tolerance for debugging
    };
});

var app = builder.Build();

// Log listening URLs on startup
var logger = app.Services.GetRequiredService<ILogger<Program>>();
if (app.Urls != null)
{
    foreach (var address in app.Urls)
    {
        logger.LogInformation("Application listening on: {address}", address);
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MealPlannerApp API V1");
    });
}
else
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

// Use the custom logging middleware
app.UseMiddleware<MealPlannerApp.LoggingMiddleware>();

app.MapControllers();

app.Run();

