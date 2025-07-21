using MealPlannerApp.Data;
using MealPlannerApp.DTOs;
using MealPlannerApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MealPlannerApp.Services
{
    public class FridgeItemService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<FridgeItemService> _logger;

        public FridgeItemService(AppDbContext context, ILogger<FridgeItemService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<FridgeItemResponseDto>> GetUserFridgeItemsAsync(Guid userId)
        {
            try
            {
                _logger.LogInformation("Getting fridge items for user: {UserId}", userId);

                // Enhanced diagnostic logging
                var totalFridgeItems = await _context.FridgeItems.CountAsync();
                _logger.LogInformation("Total fridge items in database: {Count}", totalFridgeItems);

                // Check all fridge items without filtering first
                var allFridgeItems = await _context.FridgeItems.ToListAsync();
                _logger.LogInformation("All fridge items found: {Count}", allFridgeItems.Count);

                foreach (var debugItem in allFridgeItems)
                {
                    _logger.LogInformation("Debug - FridgeItem: ID={Id}, UserId={UserId}, Name={Name}",
                        debugItem.Id, debugItem.UserId, debugItem.Name);
                }

                // First check if user exists
                var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
                if (!userExists)
                {
                    _logger.LogWarning("User not found: {UserId}", userId);
                    return new List<FridgeItemResponseDto>();
                }

                // Check if there are items with this userId in raw query
                var rawUserItems = await _context.FridgeItems
                    .Where(item => item.UserId == userId)
                    .ToListAsync();

                _logger.LogInformation("Raw query found {Count} items for user: {UserId}", rawUserItems.Count, userId);

                // Log each raw item
                foreach (var rawItem in rawUserItems)
                {
                    _logger.LogInformation("Raw item: ID={Id}, UserId={UserId}, Name={Name}, IngredientId={IngredientId}",
                        rawItem.Id, rawItem.UserId, rawItem.Name, rawItem.IngredientId);
                }

                // Now try with Include
                var items = await _context.FridgeItems
                    .Include(fi => fi.Ingredient) // Include ingredient for complete data
                    .Where(item => item.UserId == userId)
                    .ToListAsync();

                _logger.LogInformation("Query with Include found {Count} fridge items for user: {UserId}", items.Count, userId);

                // Debug logging for each item with Include
                foreach (var item in items)
                {
                    _logger.LogInformation("Include item: ID={Id}, Name={Name}, UserId={UserId}, IngredientId={IngredientId}, IngredientName={IngredientName}",
                        item.Id, item.Name, item.UserId, item.IngredientId, item.Ingredient?.Name ?? "NULL");
                }

                // Try alternative query without Include to see if that's the issue
                var itemsWithoutInclude = await _context.FridgeItems
                    .Where(item => item.UserId == userId)
                    .ToListAsync();

                _logger.LogInformation("Query WITHOUT Include found {Count} items", itemsWithoutInclude.Count);

                // Direct mapping with correct property names and null safety
                return items.Select(item => new FridgeItemResponseDto
                {
                    Id = item.Id,
                    Name = item.Name ?? "Unknown",
                    Quantity = item.Quantity ?? "0",
                    Unit = item.Unit ?? "",
                    ExpirationDate = item.ExpirationDate
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting fridge items for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<FridgeItemResponseDto?> GetFridgeItemByIdAsync(Guid itemId, Guid userId)
        {
            try
            {
                _logger.LogInformation("Getting fridge item: {ItemId} for user: {UserId}", itemId, userId);

                // Enhanced debugging
                var itemExistsAnywhere = await _context.FridgeItems.AnyAsync(fi => fi.Id == itemId);
                _logger.LogInformation("Item {ItemId} exists anywhere in database: {Exists}", itemId, itemExistsAnywhere);

                if (itemExistsAnywhere)
                {
                    var itemDetails = await _context.FridgeItems
                        .Where(fi => fi.Id == itemId)
                        .Select(fi => new { fi.Id, fi.UserId, fi.Name })
                        .FirstOrDefaultAsync();

                    _logger.LogInformation("Item details: ItemId={ItemId}, UserId={UserId}, Name={Name}",
                        itemDetails?.Id, itemDetails?.UserId, itemDetails?.Name);
                }

                // First check if user exists
                var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
                if (!userExists)
                {
                    _logger.LogWarning("User not found: {UserId}", userId);
                    return null;
                }

                var item = await _context.FridgeItems
                    .Include(fi => fi.Ingredient)
                    .FirstOrDefaultAsync(item => item.Id == itemId && item.UserId == userId);

                if (item == null)
                {
                    _logger.LogWarning("Fridge item not found: {ItemId} for user: {UserId}", itemId, userId);

                    // Check if item exists but belongs to different user
                    var itemExistsForOtherUser = await _context.FridgeItems
                        .AnyAsync(fi => fi.Id == itemId);

                    if (itemExistsForOtherUser)
                    {
                        var actualOwner = await _context.FridgeItems
                            .Where(fi => fi.Id == itemId)
                            .Select(fi => fi.UserId)
                            .FirstOrDefaultAsync();
                        _logger.LogWarning("Fridge item {ItemId} exists but belongs to user {ActualUserId}, not {RequestedUserId}",
                            itemId, actualOwner, userId);
                    }
                    else
                    {
                        _logger.LogWarning("Fridge item {ItemId} does not exist in database", itemId);
                    }

                    return null;
                }

                _logger.LogInformation("Found fridge item: {ItemName} (ID: {ItemId})", item.Name, item.Id);

                // Direct mapping with correct property names and null safety
                return new FridgeItemResponseDto
                {
                    Id = item.Id,
                    Name = item.Name ?? "Unknown",
                    Quantity = item.Quantity ?? "0",
                    Unit = item.Unit ?? "",
                    ExpirationDate = item.ExpirationDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting fridge item: {ItemId} for user: {UserId}", itemId, userId);
                throw;
            }
        }

        // Enhanced diagnostic method
        public async Task<object> GetDiagnosticInfoAsync(Guid userId)
        {
            try
            {
                _logger.LogInformation("Getting diagnostic info for user: {UserId}", userId);

                var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
                var fridgeItemCount = await _context.FridgeItems.CountAsync(fi => fi.UserId == userId);
                var totalFridgeItems = await _context.FridgeItems.CountAsync();
                var totalUsers = await _context.Users.CountAsync();

                // Get all users and their fridge item counts
                var userFridgeItemCounts = await _context.Users
                    .Select(u => new
                    {
                        UserId = u.Id,
                        UserName = u.Name,
                        FridgeItemCount = u.FridgeItems.Count()
                    })
                    .ToListAsync();

                // Get all fridge items for debugging
                var allFridgeItems = await _context.FridgeItems
                    .Select(fi => new
                    {
                        fi.Id,
                        fi.UserId,
                        fi.Name,
                        fi.Quantity,
                        fi.Unit,
                        fi.IngredientId,
                        IngredientName = fi.Ingredient != null ? fi.Ingredient.Name : "NULL"
                    })
                    .ToListAsync();

                var result = new
                {
                    UserId = userId,
                    UserExists = userExists,
                    UserFridgeItemCount = fridgeItemCount,
                    TotalFridgeItemsInDb = totalFridgeItems,
                    TotalUsersInDb = totalUsers,
                    UserFridgeItemCounts = userFridgeItemCounts,
                    AllFridgeItems = allFridgeItems,
                    SpecificUserItems = allFridgeItems.Where(fi => fi.UserId == userId).ToList()
                };

                _logger.LogInformation("Diagnostic info: User exists: {UserExists}, User items: {UserItems}, Total items: {TotalItems}",
                    userExists, fridgeItemCount, totalFridgeItems);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting diagnostic info for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<FridgeItemResponseDto> CreateFridgeItemAsync(FridgeItemCreateDto dto, Guid userId)
        {
            try
            {
                _logger.LogInformation("Creating fridge item: {IngredientName} for user: {UserId}",
                    dto.IngredientName, userId);

                // Get or create user reference
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogError("User not found: {UserId}", userId);
                    throw new ArgumentException($"User not found: {userId}");
                }

                // Find or create ingredient by name (case-insensitive)
                var ingredient = await _context.Ingredients
                    .FirstOrDefaultAsync(i => i.Name.ToLower() == dto.IngredientName.ToLower());

                if (ingredient == null)
                {
                    _logger.LogInformation("Creating new ingredient: {IngredientName}", dto.IngredientName);

                    // Create new ingredient if it doesn't exist
                    ingredient = new Ingredient
                    {
                        Id = Guid.NewGuid(),
                        Name = dto.IngredientName,
                        FridgeItems = new List<FridgeItems>(),
                        Recipes = new List<Recipes>()
                    };

                    _context.Ingredients.Add(ingredient);
                    await _context.SaveChangesAsync(); // Save ingredient first to get ID

                    _logger.LogInformation("Created new ingredient: {IngredientName} with ID: {IngredientId}",
                        ingredient.Name, ingredient.Id);
                }
                else
                {
                    _logger.LogInformation("Using existing ingredient: {IngredientName} with ID: {IngredientId}",
                        ingredient.Name, ingredient.Id);
                }

                // Create fridge item with proper ingredient relationship
                var item = new FridgeItems
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    User = user,
                    Name = dto.IngredientName,
                    Quantity = dto.Quantity ?? "1",
                    Unit = dto.Unit ?? "piece",
                    ExpirationDate = dto.ExpirationDate,
                    IngredientId = ingredient.Id, // Set the foreign key
                    Ingredient = ingredient // Set the navigation property
                };

                _context.FridgeItems.Add(item);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created fridge item: {ItemName} (ID: {ItemId}) for user: {UserId} with ingredient: {IngredientId}",
                    item.Name, item.Id, userId, ingredient.Id);

                // Verify item was created by querying it back
                var verifyItem = await _context.FridgeItems
                    .FirstOrDefaultAsync(fi => fi.Id == item.Id);

                if (verifyItem != null)
                {
                    _logger.LogInformation("Verified item creation: ID={Id}, UserId={UserId}", verifyItem.Id, verifyItem.UserId);
                }

                // Direct mapping from Entity to Response DTO
                return new FridgeItemResponseDto
                {
                    Id = item.Id,
                    Name = item.Name,
                    Quantity = item.Quantity,
                    Unit = item.Unit,
                    ExpirationDate = item.ExpirationDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating fridge item: {IngredientName} for user: {UserId}",
                    dto.IngredientName, userId);
                throw;
            }
        }

        public async Task<FridgeItemResponseDto?> UpdateFridgeItemAsync(Guid itemId, FridgeItemUpdateDto dto, Guid userId)
        {
            try
            {
                _logger.LogInformation("Updating fridge item: {ItemId} for user: {UserId}", itemId, userId);

                var existingItem = await _context.FridgeItems
                    .Include(fi => fi.Ingredient)
                    .FirstOrDefaultAsync(item => item.Id == itemId && item.UserId == userId);

                if (existingItem == null)
                {
                    _logger.LogWarning("Fridge item not found for update: {ItemId} for user: {UserId}", itemId, userId);
                    return null;
                }

                // Check if ingredient name has changed
                if (existingItem.Name.ToLower() != dto.Name.ToLower())
                {
                    _logger.LogInformation("Ingredient name changed from {OldName} to {NewName}",
                        existingItem.Name, dto.Name);

                    // Find or create new ingredient
                    var ingredient = await _context.Ingredients
                        .FirstOrDefaultAsync(i => i.Name.ToLower() == dto.Name.ToLower());

                    if (ingredient == null)
                    {
                        _logger.LogInformation("Creating new ingredient during update: {IngredientName}", dto.Name);

                        // Create new ingredient if it doesn't exist
                        ingredient = new Ingredient
                        {
                            Id = Guid.NewGuid(),
                            Name = dto.Name,
                            FridgeItems = new List<FridgeItems>(),
                            Recipes = new List<Recipes>()
                        };

                        _context.Ingredients.Add(ingredient);
                        await _context.SaveChangesAsync(); // Save ingredient first

                        _logger.LogInformation("Created new ingredient during update: {IngredientName} with ID: {IngredientId}",
                            ingredient.Name, ingredient.Id);
                    }

                    // Update ingredient relationship
                    existingItem.IngredientId = ingredient.Id;
                    existingItem.Ingredient = ingredient;
                }

                // Update properties with null safety
                existingItem.Name = dto.Name;
                existingItem.Quantity = dto.Quantity ?? existingItem.Quantity;
                existingItem.Unit = dto.Unit ?? existingItem.Unit;
                existingItem.ExpirationDate = dto.ExpirationDate;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated fridge item: {ItemId} for user: {UserId}", itemId, userId);

                // Direct mapping to response DTO
                return new FridgeItemResponseDto
                {
                    Id = existingItem.Id,
                    Name = existingItem.Name,
                    Quantity = existingItem.Quantity,
                    Unit = existingItem.Unit,
                    ExpirationDate = existingItem.ExpirationDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating fridge item: {ItemId} for user: {UserId}", itemId, userId);
                throw;
            }
        }

        public async Task<bool> DeleteFridgeItemAsync(Guid itemId, Guid userId)
        {
            try
            {
                _logger.LogInformation("Deleting fridge item: {ItemId} for user: {UserId}", itemId, userId);

                var item = await _context.FridgeItems
                    .FirstOrDefaultAsync(item => item.Id == itemId && item.UserId == userId);

                if (item == null)
                {
                    _logger.LogWarning("Fridge item not found for deletion: {ItemId} for user: {UserId}", itemId, userId);
                    return false;
                }

                _context.FridgeItems.Remove(item);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted fridge item: {ItemId} for user: {UserId}", itemId, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting fridge item: {ItemId} for user: {UserId}", itemId, userId);
                throw;
            }
        }
    }
}