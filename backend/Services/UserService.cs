using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MealPlannerApp.Data;
using MealPlannerApp.DTOs;
using MealPlannerApp.Models;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MealPlannerApp.Config;
using Microsoft.Extensions.Logging;

namespace MealPlannerApp.Services
{
    public class UserService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(AppDbContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<User> LoginOrRegisterGoogleUser(string googleId, string name, string email, string? avatarUrl)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);
            if (user != null)
                return user;

            user = new User
            {
                Id = Guid.NewGuid(),
                GoogleId = googleId,
                Name = name,
                Email = email,
                AvatarUrl = avatarUrl,
                FridgeItems = new List<FridgeItems>(),
                SavedRecipes = new List<Recipes>(),
                ShoppingListItems = new List<ShoppingListItems>()
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> GetUserById(Guid id)
        {
            var user = await _context.Users
                .Include(u => u.FridgeItems)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                throw new Exception("User not found");
            return user;
        }

        public async Task<UserResponseDto> GetUserResponseById(Guid id)
        {
            var user = await _context.Users
                .Include(u => u.FridgeItems)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                throw new Exception("User not found");

            return new UserResponseDto
            {
                Id = user.Id,
                GoogleId = user.GoogleId,
                Name = user.Name,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl,
                FridgeItems = user.FridgeItems.Select(item => new FridgeItemResponseDto
                {
                    Id = item.Id,
                    Name = item.Name, // Model property is Name, DTO property is Name
                    Quantity = item.Quantity,
                    Unit = item.Unit,
                    ExpirationDate = item.ExpirationDate
                }).ToList()
            };
        }

        public async Task<UserResponseDto?> GetUserByIdAsync(Guid userId)
        {
            var user = await _context.Users
                .Include(u => u.FridgeItems)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return null;

            // Direct mapping with all required properties using correct names
            return new UserResponseDto
            {
                Id = user.Id,
                GoogleId = user.GoogleId,
                Name = user.Name,
                Email = user.Email,
                FridgeItems = user.FridgeItems.Select(item => new FridgeItemResponseDto
                {
                    Id = item.Id,
                    Name = item.Name, // Model property is Name, DTO property is Name
                    Quantity = item.Quantity,
                    Unit = item.Unit,
                    ExpirationDate = item.ExpirationDate
                }).ToList()
            };
        }

        public async Task<UserResponseDto?> GetUserByEmailAsync(string email)
        {
            var user = await _context.Users
                .Include(u => u.FridgeItems)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null) return null;

            return new UserResponseDto
            {
                Id = user.Id,
                GoogleId = user.GoogleId,
                Name = user.Name,
                Email = user.Email,
                FridgeItems = user.FridgeItems.Select(item => new FridgeItemResponseDto
                {
                    Id = item.Id,
                    Name = item.Name, // Model property is Name, DTO property is Name
                    Quantity = item.Quantity,
                    Unit = item.Unit,
                    ExpirationDate = item.ExpirationDate
                }).ToList()
            };
        }

        public async Task<User> CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> FindOrCreateUserFromGoogleAsync(UserLoginDto userLoginDto)
        {
            var googleId = userLoginDto.GoogleId;
            var name = userLoginDto.Name;
            var email = userLoginDto.Email;
            var avatarUrl = userLoginDto.AvatarUrl;

            if (string.IsNullOrEmpty(googleId) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email))
            {
                _logger.LogError("Invalid user login data: GoogleId, Name, and Email are required.");
                return null;
            }

            try
            {
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.GoogleId == googleId);

                if (existingUser != null)
                {
                    _logger.LogInformation("Found existing user with GoogleId: {GoogleId}", googleId);
                    return existingUser;
                }

                var newUser = new User
                {
                    Id = Guid.NewGuid(),
                    GoogleId = googleId,
                    Name = name,
                    Email = email,
                    AvatarUrl = avatarUrl ?? string.Empty,
                    FridgeItems = new List<FridgeItems>(),
                    SavedRecipes = new List<Recipes>(),
                    ShoppingListItems = new List<ShoppingListItems>()
                };

                _logger.LogInformation("Creating new user with GoogleId: {GoogleId}, Email: {Email}", googleId, email);
                return await CreateUserAsync(newUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while finding or creating user with GoogleId: {GoogleId}", googleId);
                return null;
            }
        }

        public string GenerateJwtToken(User user, IConfiguration config)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name, user.Name)
                // Add more claims as needed
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AppConfig.JwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: AppConfig.JwtIssuer,
                audience: AppConfig.JwtAudience,
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}