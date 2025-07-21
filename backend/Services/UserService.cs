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
                    Name = item.Name,
                    Quantity = item.Quantity,
                    Unit = item.Unit,
                    ExpirationDate = item.ExpirationDate
                }).ToList()
            };
        }

        public string GenerateJwtToken(User user, IConfiguration config)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name, user.Name),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AppConfig.JwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: AppConfig.JwtIssuer,
                audience: AppConfig.JwtAudience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            _logger.LogInformation("Generated JWT token for user {UserId} with expiry {Expiry}",
                user.Id, DateTime.UtcNow.AddDays(7));

            return tokenString;
        }
    }
}