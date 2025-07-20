using System;
using System.Collections.Generic;
using MealPlannerApp.Models;

namespace MealPlannerApp.Tests.Helpers
{
    public static class TestDataHelper
    {
        public static User CreateTestUser(string googleId = "123456789", string name = "Test User", string email = "test@example.com")
        {
            var userId = Guid.NewGuid();
            return new User
            {
                Id = userId,
                GoogleId = googleId,
                Name = name,
                Email = email,
                AvatarUrl = "http://example.com/avatar.jpg",
                FridgeItems = new List<FridgeItems>(),
                SavedRecipes = new List<Recipes>(),
                ShoppingListItems = new List<ShoppingListItems>()
            };
        }

        public static FridgeItems CreateTestFridgeItem(Guid userId, string name = "Test Item", string quantity = "1", string unit = "piece")
        {
            return new FridgeItems
            {
                Id = Guid.NewGuid(),
                Name = name,
                Quantity = quantity,
                Unit = unit,
                ExpirationDate = DateTime.Now.AddDays(7),
                UserId = userId,
                User = null!, // Will be set by EF
                IngredientId = Guid.NewGuid()
            };
        }
    }
}
