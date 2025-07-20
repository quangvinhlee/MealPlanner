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
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace MealPlannerApp.Tests.Integration
{
    /// <summary>
    /// Integration tests for the actual UserService class
    /// These tests use in-memory database and test the real service implementation
    /// </summary>
    [TestFixture]
    public class UserServiceIntegrationTests
    {
        private AppDbContext _context = null!;
        private UserService _userService = null!;
        private IMapper _mapper = null!;
        private IConfiguration _configuration = null!;

        [SetUp]
        public void Setup()
        {
            // Setup in-memory database with unique name for each test
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new AppDbContext(options);

            // Setup AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AutoMapperProfile>();
            });
            _mapper = config.CreateMapper();

            // Setup configuration for JWT
            var inMemorySettings = new Dictionary<string, string?>
            {
                {"Jwt:Key", "ThisIsATestKeyThatIsLongEnoughForHS256AlgorithmToWork123456"},
                {"Jwt:Issuer", "TestIssuer"},
                {"Jwt:Audience", "TestAudience"}
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();

            // Initialize AppConfig with the test configuration
            MealPlannerApp.Config.AppConfig.Initialize(_configuration);

            // Create the actual UserService instance
            _userService = new UserService(_context, _mapper);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task LoginOrRegisterGoogleUser_WhenUserDoesNotExist_CreatesNewUser()
        {
            // Arrange
            var googleId = "new_user_123";
            var name = "New User";
            var email = "newuser@example.com";
            var avatarUrl = "https://example.com/avatar.jpg";

            // Act
            var result = await _userService.LoginOrRegisterGoogleUser(googleId, name, email, avatarUrl);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.GoogleId, Is.EqualTo(googleId));
            Assert.That(result.Name, Is.EqualTo(name));
            Assert.That(result.Email, Is.EqualTo(email));
            Assert.That(result.AvatarUrl, Is.EqualTo(avatarUrl));
            Assert.That(result.Id, Is.Not.EqualTo(Guid.Empty));

            // Verify user was actually saved to database
            var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);
            Assert.That(userInDb, Is.Not.Null);
            Assert.That(userInDb!.Id, Is.EqualTo(result.Id));
        }

        [Test]
        public async Task LoginOrRegisterGoogleUser_WhenUserExists_ReturnsExistingUser()
        {
            // Arrange - Create existing user
            var existingUser = new User
            {
                Id = Guid.NewGuid(),
                GoogleId = "existing_user_456",
                Name = "Existing User",
                Email = "existing@example.com",
                AvatarUrl = "https://example.com/existing.jpg",
                FridgeItems = new List<FridgeItems>(),
                SavedRecipes = new List<Recipes>(),
                ShoppingListItems = new List<ShoppingListItems>()
            };

            _context.Users.Add(existingUser);
            await _context.SaveChangesAsync();

            // Act - Try to register the same user again
            var result = await _userService.LoginOrRegisterGoogleUser(
                existingUser.GoogleId,
                "Updated Name", // Different name
                existingUser.Email,
                existingUser.AvatarUrl);

            // Assert - Should return existing user, not create new one
            Assert.That(result.Id, Is.EqualTo(existingUser.Id));
            Assert.That(result.GoogleId, Is.EqualTo(existingUser.GoogleId));
            Assert.That(result.Name, Is.EqualTo(existingUser.Name)); // Should keep original name

            // Verify only one user exists in database
            var userCount = await _context.Users.CountAsync(u => u.GoogleId == existingUser.GoogleId);
            Assert.That(userCount, Is.EqualTo(1));
        }

        [Test]
        public async Task GetUserById_WhenUserExists_ReturnsUserWithFridgeItems()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                GoogleId = "test_user_789",
                Name = "Test User",
                Email = "test@example.com",
                FridgeItems = new List<FridgeItems>(),
                SavedRecipes = new List<Recipes>(),
                ShoppingListItems = new List<ShoppingListItems>()
            };

            // Add some fridge items
            user.FridgeItems.Add(new FridgeItems
            {
                Id = Guid.NewGuid(),
                Name = "Milk",
                Quantity = "1",
                Unit = "Liter",
                ExpirationDate = DateTime.Now.AddDays(7),
                UserId = userId,
                User = user,
                IngredientId = Guid.NewGuid()
            });

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.GetUserById(userId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(userId));
            Assert.That(result.FridgeItems, Is.Not.Null);
            Assert.That(result.FridgeItems.Count, Is.EqualTo(1));
            Assert.That(result.FridgeItems.First().Name, Is.EqualTo("Milk"));
        }

        [Test]
        public async Task GetUserById_WhenUserDoesNotExist_ThrowsException()
        {
            // Arrange
            var nonExistentUserId = Guid.NewGuid();

            // Act & Assert
            Exception exception = null!;
            try
            {
                await _userService.GetUserById(nonExistentUserId);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            Assert.That(exception, Is.Not.Null);
            Assert.That(exception.Message, Is.EqualTo("User not found"));
        }

        [Test]
        public async Task GetUserResponseById_WhenUserExists_ReturnsUserResponseDto()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                GoogleId = "dto_test_user",
                Name = "DTO Test User",
                Email = "dtotest@example.com",
                AvatarUrl = "https://example.com/dto.jpg",
                FridgeItems = new List<FridgeItems>(),
                SavedRecipes = new List<Recipes>(),
                ShoppingListItems = new List<ShoppingListItems>()
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.GetUserResponseById(userId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.TypeOf<UserResponseDto>());
            Assert.That(result.Id, Is.EqualTo(userId));
            Assert.That(result.GoogleId, Is.EqualTo("dto_test_user"));
            Assert.That(result.Name, Is.EqualTo("DTO Test User"));
            Assert.That(result.Email, Is.EqualTo("dtotest@example.com"));
            Assert.That(result.FridgeItems, Is.Not.Null);
        }

        [Test]
        public void GenerateJwtToken_WithValidUser_ReturnsValidJwtToken()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                GoogleId = "jwt_test_user",
                Name = "JWT Test User",
                Email = "jwttest@example.com",
                FridgeItems = new List<FridgeItems>(),
                SavedRecipes = new List<Recipes>(),
                ShoppingListItems = new List<ShoppingListItems>()
            };

            // Act
            var token = _userService.GenerateJwtToken(user, _configuration);

            // Assert
            Assert.That(token, Is.Not.Null);
            Assert.That(token, Is.Not.Empty);

            // JWT should have 3 parts separated by dots
            var parts = token.Split('.');
            Assert.That(parts.Length, Is.EqualTo(3));

            // Each part should not be empty
            Assert.That(parts[0], Is.Not.Empty, "Header should not be empty");
            Assert.That(parts[1], Is.Not.Empty, "Payload should not be empty");
            Assert.That(parts[2], Is.Not.Empty, "Signature should not be empty");
        }

        [Test]
        public async Task LoginOrRegisterGoogleUser_WithNullAvatarUrl_CreatesUserSuccessfully()
        {
            // Arrange
            var googleId = "no_avatar_user";
            var name = "No Avatar User";
            var email = "noavatar@example.com";

            // Act
            var result = await _userService.LoginOrRegisterGoogleUser(googleId, name, email, null);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.GoogleId, Is.EqualTo(googleId));
            Assert.That(result.Name, Is.EqualTo(name));
            Assert.That(result.Email, Is.EqualTo(email));
            Assert.That(result.AvatarUrl, Is.Null);
        }

        [Test]
        public async Task ConcurrentUserRegistration_SameGoogleId_ShouldNotCreateDuplicates()
        {
            // Arrange
            var googleId = "concurrent_test_user";
            var name = "Concurrent Test User";
            var email = "concurrent@example.com";

            // Act - Simulate concurrent calls
            var task1 = _userService.LoginOrRegisterGoogleUser(googleId, name, email, null);
            var task2 = _userService.LoginOrRegisterGoogleUser(googleId, name, email, null);

            var results = await Task.WhenAll(task1, task2);

            // Assert
            var usersInDb = await _context.Users.Where(u => u.GoogleId == googleId).ToListAsync();

            // Should only create one user despite concurrent calls
            Assert.That(usersInDb.Count, Is.EqualTo(1));

            // Both results should be the same user (though timing might affect which one gets the actual DB record)
            Assert.That(results[0].GoogleId, Is.EqualTo(googleId));
            Assert.That(results[1].GoogleId, Is.EqualTo(googleId));
        }
    }
}
