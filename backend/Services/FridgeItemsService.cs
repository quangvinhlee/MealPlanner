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

            var fridgeItem = new FridgeItems
            {
                Name = dto.Name,
                Quantity = dto.Quantity ?? "1",
                Unit = dto.Unit ?? "pcs",
                ExpirationDate = dto.ExpirationDate,
                UserId = userId,
                User = null!
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
    }
}