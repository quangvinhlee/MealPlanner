using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MealPlannerApp.Data;
using MealPlannerApp.Models;
using Microsoft.EntityFrameworkCore;

namespace MealPlannerApp.Services
{
    public class UserService
    {
        private readonly AppDbContext _context;
        public UserService(AppDbContext context)
        {
            _context = context;
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
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                throw new Exception("User not found");
            }
            return user;
        }
    }
}