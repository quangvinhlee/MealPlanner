using MealPlannerApp.Models;
using MealPlannerApp.Data;
using MealPlannerApp.DTOs;
using Microsoft.EntityFrameworkCore;

namespace MealPlannerApp.Services
{
    public class FridgeItemsService
    {
        private readonly AppDbContext _context;
        public FridgeItemsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<FridgeItemResponseDto> AddFridgeItem(FridgeItemCreateDto dto, Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new Exception("User not found");

            var ingredient = await _context.Ingredients.FindAsync(dto.IngredientId);
            if (ingredient == null)
                throw new Exception("Ingredient not found");

            var fridgeItem = new FridgeItems
            {
                Name = dto.Name,
                Quantity = dto.Quantity ?? "1",
                Unit = dto.Unit ?? "pcs",
                ExpirationDate = dto.ExpirationDate,
                UserId = userId,
                User = user,
                IngredientId = ingredient.Id,
                Ingredient = ingredient
            };
            _context.FridgeItems.Add(fridgeItem);
            await _context.SaveChangesAsync();

            return new FridgeItemResponseDto
            {
                Id = fridgeItem.Id,
                Name = fridgeItem.Name,
                Quantity = fridgeItem.Quantity,
                Unit = fridgeItem.Unit,
                ExpirationDate = fridgeItem.ExpirationDate
            };
        }

        public async Task<List<FridgeItemResponseDto>> GetFridgeItemResponsesByUserId(Guid userId)
        {
            var items = await _context.FridgeItems
                .Where(f => f.UserId == userId)
                .ToListAsync();

            return items.Select(item => new FridgeItemResponseDto
            {
                Id = item.Id,
                Name = item.Name,
                Quantity = item.Quantity,
                Unit = item.Unit,
                ExpirationDate = item.ExpirationDate
            }).ToList();
        }

        public async Task<FridgeItemResponseDto?> UpdateFridgeItem(Guid userId, FridgeItemUpdateDto dto)
        {
            var item = await _context.FridgeItems
                .FirstOrDefaultAsync(f => f.Id == dto.Id && f.UserId == userId);

            if (item == null)
                return null;

            if (dto.Name != null) item.Name = dto.Name;
            if (dto.Quantity != null) item.Quantity = dto.Quantity;
            if (dto.Unit != null) item.Unit = dto.Unit;
            if (dto.ExpirationDate != null) item.ExpirationDate = dto.ExpirationDate;

            await _context.SaveChangesAsync();

            return new FridgeItemResponseDto
            {
                Id = item.Id,
                Name = item.Name,
                Quantity = item.Quantity,
                Unit = item.Unit,
                ExpirationDate = item.ExpirationDate
            };
        }
    }
}