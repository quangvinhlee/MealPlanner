using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MealPlannerApp.Data;
using MealPlannerApp.DTOs;
using MealPlannerApp.Models;
using MealPlannerApp.Services;
using MealPlannerApp.Mapping;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace MealPlannerApp.Tests.Integration
{
    /// <summary>
    /// Integration tests for the FridgeItemsService class
    /// These tests use in-memory database and test the real service implementation
    /// </summary>
    [TestFixture]
    public class FridgeItemsServiceIntegrationTests
    {
        private AppDbContext _context = null!;
        private FridgeItemsService _fridgeItemsService = null!;
        private IMapper _mapper = null!;
        private User _testUser = null!;

        [SetUp]
        public void Setup()
        {
            // Setup in-memory database with unique name for each test
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .EnableSensitiveDataLogging() // Enable for better error messages
                .Options;

            _context = new AppDbContext(options);

            // Setup AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AutoMapperProfile>();
            });
            _mapper = config.CreateMapper();

            // Create the original FridgeItemsService instance
            _fridgeItemsService = new FridgeItemsService(_context, _mapper);

            // Create a test user for all tests
            _testUser = new User
            {
                Id = Guid.NewGuid(),
                GoogleId = "test_user_123",
                Name = "Test User",
                Email = "test@example.com",
                FridgeItems = new List<FridgeItems>(),
                SavedRecipes = new List<Recipes>(),
                ShoppingListItems = new List<ShoppingListItems>()
            };

            _context.Users.Add(_testUser);
            _context.SaveChanges();
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task AddFridgeItem_WithNewIngredient_CreatesIngredientAndFridgeItem()
        {
            // Arrange
            var dto = new FridgeItemCreateDto
            {
                IngredientName = "tomato",
                Quantity = "3",
                Unit = "pcs",
                ExpirationDate = DateTime.Now.AddDays(5)
            };

            // Act
            var result = await _fridgeItemsService.AddFridgeItem(dto, _testUser.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("Tomato")); // Should be capitalized
            Assert.That(result.Quantity, Is.EqualTo("3"));
            Assert.That(result.Unit, Is.EqualTo("pcs"));
            Assert.That(result.ExpirationDate, Is.EqualTo(dto.ExpirationDate));

            // Verify ingredient was created in database
            var ingredient = await _context.Ingredients.FirstOrDefaultAsync(i => i.Name == "Tomato");
            Assert.That(ingredient, Is.Not.Null);

            // Verify fridge item was created in database
            var fridgeItem = await _context.FridgeItems.FirstOrDefaultAsync(f => f.UserId == _testUser.Id);
            Assert.That(fridgeItem, Is.Not.Null);
            Assert.That(fridgeItem!.IngredientId, Is.EqualTo(ingredient!.Id));
        }

        [Test]
        public async Task AddFridgeItem_WithExistingIngredient_ReusesIngredient()
        {
            // Arrange - Create existing ingredient
            var existingIngredient = new Ingredient
            {
                Name = "Banana",
                FridgeItems = new List<FridgeItems>(),
                Recipes = new List<Recipes>()
            };
            _context.Ingredients.Add(existingIngredient);
            await _context.SaveChangesAsync();

            var dto = new FridgeItemCreateDto
            {
                IngredientName = "banana", // Different case
                Quantity = "6",
                Unit = "pcs"
            };

            // Act
            var result = await _fridgeItemsService.AddFridgeItem(dto, _testUser.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("Banana"));

            // Verify only one ingredient exists
            var ingredientCount = await _context.Ingredients.CountAsync(i => i.Name.ToLower() == "banana");
            Assert.That(ingredientCount, Is.EqualTo(1));

            // Verify fridge item uses existing ingredient
            var fridgeItem = await _context.FridgeItems
                .Include(f => f.Ingredient)
                .FirstOrDefaultAsync(f => f.UserId == _testUser.Id);
            Assert.That(fridgeItem, Is.Not.Null);
            Assert.That(fridgeItem!.IngredientId, Is.EqualTo(existingIngredient.Id));
        }

        [Test]
        public async Task AddFridgeItem_WithDefaultValues_UsesDefaults()
        {
            // Arrange
            var dto = new FridgeItemCreateDto
            {
                IngredientName = "apple"
                // No quantity, unit, or expiration date provided
            };

            // Act
            var result = await _fridgeItemsService.AddFridgeItem(dto, _testUser.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("Apple"));
            Assert.That(result.Quantity, Is.EqualTo("1")); // Default quantity
            Assert.That(result.Unit, Is.EqualTo("pcs")); // Default unit
            Assert.That(result.ExpirationDate, Is.Null);
        }

        [Test]
        public async Task AddFridgeItem_WithNonExistentUser_ThrowsException()
        {
            // Arrange
            var nonExistentUserId = Guid.NewGuid();
            var dto = new FridgeItemCreateDto
            {
                IngredientName = "orange"
            };

            // Act & Assert
            var exception = Assert.ThrowsAsync<Exception>(async () =>
                await _fridgeItemsService.AddFridgeItem(dto, nonExistentUserId));

            Assert.That(exception.Message, Is.EqualTo("User not found"));
        }

        [Test]
        public async Task AddFridgeItem_WithVariousCasing_NormalizesCorrectly()
        {
            // Arrange & Act - Add items with different casing
            var dto1 = new FridgeItemCreateDto { IngredientName = "CARROT" };
            var dto2 = new FridgeItemCreateDto { IngredientName = "carrot" };
            var dto3 = new FridgeItemCreateDto { IngredientName = "Carrot" };

            var result1 = await _fridgeItemsService.AddFridgeItem(dto1, _testUser.Id);
            var result2 = await _fridgeItemsService.AddFridgeItem(dto2, _testUser.Id);
            var result3 = await _fridgeItemsService.AddFridgeItem(dto3, _testUser.Id);

            // Assert - All should use the same ingredient
            Assert.That(result1.Name, Is.EqualTo("Carrot"));
            Assert.That(result2.Name, Is.EqualTo("Carrot"));
            Assert.That(result3.Name, Is.EqualTo("Carrot"));

            // Verify only one ingredient was created
            var ingredientCount = await _context.Ingredients.CountAsync(i => i.Name.ToLower() == "carrot");
            Assert.That(ingredientCount, Is.EqualTo(1));

            // Verify all fridge items were created
            var fridgeItemCount = await _context.FridgeItems.CountAsync(f => f.UserId == _testUser.Id);
            Assert.That(fridgeItemCount, Is.EqualTo(3));
        }

        [Test]
        public async Task GetFridgeItemResponsesByUserId_WithMultipleItems_ReturnsAllUserItems()
        {
            // Arrange - Create multiple ingredients and fridge items
            var ingredient1 = new Ingredient { Name = "Milk", FridgeItems = new List<FridgeItems>(), Recipes = new List<Recipes>() };
            var ingredient2 = new Ingredient { Name = "Bread", FridgeItems = new List<FridgeItems>(), Recipes = new List<Recipes>() };
            _context.Ingredients.AddRange(ingredient1, ingredient2);
            await _context.SaveChangesAsync();

            var fridgeItem1 = new FridgeItems
            {
                Name = "Milk",
                Quantity = "1",
                Unit = "Liter",
                ExpirationDate = DateTime.Now.AddDays(3),
                UserId = _testUser.Id,
                User = _testUser,
                IngredientId = ingredient1.Id
            };

            var fridgeItem2 = new FridgeItems
            {
                Name = "Bread",
                Quantity = "1",
                Unit = "Loaf",
                ExpirationDate = DateTime.Now.AddDays(2),
                UserId = _testUser.Id,
                User = _testUser,
                IngredientId = ingredient2.Id
            };

            _context.FridgeItems.AddRange(fridgeItem1, fridgeItem2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _fridgeItemsService.GetFridgeItemResponsesByUserId(_testUser.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));

            var milkItem = result.FirstOrDefault(r => r.Name == "Milk");
            var breadItem = result.FirstOrDefault(r => r.Name == "Bread");

            Assert.That(milkItem, Is.Not.Null);
            Assert.That(milkItem!.Quantity, Is.EqualTo("1"));
            Assert.That(milkItem.Unit, Is.EqualTo("Liter"));

            Assert.That(breadItem, Is.Not.Null);
            Assert.That(breadItem!.Quantity, Is.EqualTo("1"));
            Assert.That(breadItem.Unit, Is.EqualTo("Loaf"));
        }

        [Test]
        public async Task GetFridgeItemResponsesByUserId_WithNoItems_ReturnsEmptyList()
        {
            // Arrange - No fridge items for user

            // Act
            var result = await _fridgeItemsService.GetFridgeItemResponsesByUserId(_testUser.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetFridgeItemResponsesByUserId_WithDifferentUsers_ReturnsOnlyUserItems()
        {
            // Arrange - Create another user
            var anotherUser = new User
            {
                Id = Guid.NewGuid(),
                GoogleId = "another_user",
                Name = "Another User",
                Email = "another@example.com",
                FridgeItems = new List<FridgeItems>(),
                SavedRecipes = new List<Recipes>(),
                ShoppingListItems = new List<ShoppingListItems>()
            };
            _context.Users.Add(anotherUser);
            await _context.SaveChangesAsync();

            // Create ingredient
            var ingredient = new Ingredient { Name = "Cheese", FridgeItems = new List<FridgeItems>(), Recipes = new List<Recipes>() };
            _context.Ingredients.Add(ingredient);
            await _context.SaveChangesAsync();

            // Add fridge items for different users
            var testUserItem = new FridgeItems
            {
                Name = "Cheese",
                Quantity = "200",
                Unit = "g",
                UserId = _testUser.Id,
                User = _testUser,
                IngredientId = ingredient.Id
            };

            var anotherUserItem = new FridgeItems
            {
                Name = "Cheese",
                Quantity = "500",
                Unit = "g",
                UserId = anotherUser.Id,
                User = anotherUser,
                IngredientId = ingredient.Id
            };

            _context.FridgeItems.AddRange(testUserItem, anotherUserItem);
            await _context.SaveChangesAsync();

            // Act
            var result = await _fridgeItemsService.GetFridgeItemResponsesByUserId(_testUser.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().Quantity, Is.EqualTo("200"));
        }

        [Test]
        public async Task UpdateFridgeItem_WithValidData_UpdatesItem()
        {
            // Arrange - Create fridge item
            var ingredient = new Ingredient { Name = "Potato", FridgeItems = new List<FridgeItems>(), Recipes = new List<Recipes>() };
            _context.Ingredients.Add(ingredient);
            await _context.SaveChangesAsync();

            var fridgeItem = new FridgeItems
            {
                Name = "Potato",
                Quantity = "5",
                Unit = "kg",
                ExpirationDate = DateTime.Now.AddDays(10),
                UserId = _testUser.Id,
                User = _testUser,
                IngredientId = ingredient.Id
            };
            _context.FridgeItems.Add(fridgeItem);
            await _context.SaveChangesAsync();

            var updateDto = new FridgeItemUpdateDto
            {
                Id = fridgeItem.Id,
                Name = "Sweet Potato",
                Quantity = "3",
                Unit = "kg",
                ExpirationDate = DateTime.Now.AddDays(7)
            };

            // Act
            var result = await _fridgeItemsService.UpdateFridgeItem(_testUser.Id, updateDto);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Name, Is.EqualTo("Sweet Potato"));
            Assert.That(result.Quantity, Is.EqualTo("3"));
            Assert.That(result.Unit, Is.EqualTo("kg"));
            Assert.That(result.ExpirationDate, Is.EqualTo(updateDto.ExpirationDate));

            // Verify in database
            var updatedItem = await _context.FridgeItems.FindAsync(fridgeItem.Id);
            Assert.That(updatedItem, Is.Not.Null);
            Assert.That(updatedItem!.Name, Is.EqualTo("Sweet Potato"));
            Assert.That(updatedItem.Quantity, Is.EqualTo("3"));
        }

        [Test]
        public async Task UpdateFridgeItem_WithPartialData_UpdatesOnlyProvidedFields()
        {
            // Arrange - Create fridge item
            var ingredient = new Ingredient { Name = "Onion", FridgeItems = new List<FridgeItems>(), Recipes = new List<Recipes>() };
            _context.Ingredients.Add(ingredient);
            await _context.SaveChangesAsync();

            var fridgeItem = new FridgeItems
            {
                Name = "Onion",
                Quantity = "2",
                Unit = "kg",
                ExpirationDate = DateTime.Now.AddDays(14),
                UserId = _testUser.Id,
                User = _testUser,
                IngredientId = ingredient.Id
            };
            _context.FridgeItems.Add(fridgeItem);
            await _context.SaveChangesAsync();

            var updateDto = new FridgeItemUpdateDto
            {
                Id = fridgeItem.Id,
                Quantity = "1" // Only updating quantity
            };

            // Act
            var result = await _fridgeItemsService.UpdateFridgeItem(_testUser.Id, updateDto);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Name, Is.EqualTo("Onion")); // Unchanged
            Assert.That(result.Quantity, Is.EqualTo("1")); // Updated
            Assert.That(result.Unit, Is.EqualTo("kg")); // Unchanged
            Assert.That(result.ExpirationDate, Is.EqualTo(fridgeItem.ExpirationDate)); // Unchanged
        }

        [Test]
        public async Task UpdateFridgeItem_WithNonExistentItem_ReturnsNull()
        {
            // Arrange
            var updateDto = new FridgeItemUpdateDto
            {
                Id = Guid.NewGuid(),
                Name = "Non-existent"
            };

            // Act
            var result = await _fridgeItemsService.UpdateFridgeItem(_testUser.Id, updateDto);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task UpdateFridgeItem_WithWrongUserId_ReturnsNull()
        {
            // Arrange - Create fridge item for test user
            var ingredient = new Ingredient { Name = "Garlic", FridgeItems = new List<FridgeItems>(), Recipes = new List<Recipes>() };
            _context.Ingredients.Add(ingredient);
            await _context.SaveChangesAsync();

            var fridgeItem = new FridgeItems
            {
                Name = "Garlic",
                Quantity = "1",
                Unit = "head",
                UserId = _testUser.Id,
                User = _testUser,
                IngredientId = ingredient.Id
            };
            _context.FridgeItems.Add(fridgeItem);
            await _context.SaveChangesAsync();

            var updateDto = new FridgeItemUpdateDto
            {
                Id = fridgeItem.Id,
                Name = "Updated Garlic"
            };

            var differentUserId = Guid.NewGuid();

            // Act
            var result = await _fridgeItemsService.UpdateFridgeItem(differentUserId, updateDto);

            // Assert
            Assert.That(result, Is.Null);

            // Verify item was not updated
            var unchangedItem = await _context.FridgeItems.FindAsync(fridgeItem.Id);
            Assert.That(unchangedItem!.Name, Is.EqualTo("Garlic")); // Should remain unchanged
        }

        [Test]
        public async Task AddFridgeItem_WithWhitespaceInIngredientName_TrimsAndCapitalizes()
        {
            // Arrange
            var dto = new FridgeItemCreateDto
            {
                IngredientName = "  spinach  " // With leading/trailing whitespace
            };

            // Act
            var result = await _fridgeItemsService.AddFridgeItem(dto, _testUser.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("Spinach")); // Should be trimmed and capitalized

            // Verify ingredient in database
            var ingredient = await _context.Ingredients.FirstOrDefaultAsync(i => i.Name == "Spinach");
            Assert.That(ingredient, Is.Not.Null);
        }

        [Test]
        public async Task AddFridgeItem_WithEmptyIngredientName_ThrowsException()
        {
            // Arrange
            var dto = new FridgeItemCreateDto
            {
                IngredientName = ""
            };

            // Act & Assert
            // The service doesn't explicitly handle empty names, but the database constraint should catch it
            var exception = Assert.ThrowsAsync<Exception>(async () =>
                await _fridgeItemsService.AddFridgeItem(dto, _testUser.Id));

            // The exact exception type may vary depending on the database constraints
            Assert.That(exception, Is.Not.Null);
        }
    }
}
