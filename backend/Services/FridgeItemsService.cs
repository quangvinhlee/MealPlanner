using MealPlannerApp.Models;
using MealPlannerApp.Data;
using MealPlannerApp.DTOs;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using System.Globalization;

namespace MealPlannerApp.Services
{
    public class FridgeItemsService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public FridgeItemsService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        private string NormalizeIngredientName(string name)
        {
            return name.Trim().ToLowerInvariant();
        }

        private string CapitalizeFirstLetter(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name.ToLower());
        }

        public async Task<FridgeItemResponseDto> AddFridgeItem(FridgeItemCreateDto dto, Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new Exception("User not found");

            // Check for existing ingredient case-insensitively
            var ingredient = await _context.Ingredients
                .FirstOrDefaultAsync(i => EF.Functions.Collate(i.Name, "SQL_Latin1_General_CP1_CI_AS") ==
                                          EF.Functions.Collate(dto.IngredientName, "SQL_Latin1_General_CP1_CI_AS"));

            // Create ingredient if it doesn't exist
            if (ingredient == null)
            {
                ingredient = new Ingredient
                {
                    Name = CapitalizeFirstLetter(dto.IngredientName),
                    FridgeItems = null!,
                    Recipes = null!
                };
                _context.Ingredients.Add(ingredient);
                await _context.SaveChangesAsync();
            }

            var fridgeItem = new FridgeItems
            {
                Name = ingredient.Name,  // Use the ingredient name
                Quantity = dto.Quantity ?? "1",
                Unit = dto.Unit ?? "pcs",
                ExpirationDate = dto.ExpirationDate,
                UserId = userId,
                User = null!,
                IngredientId = ingredient.Id
            };
            _context.FridgeItems.Add(fridgeItem);
            await _context.SaveChangesAsync();

            return _mapper.Map<FridgeItemResponseDto>(fridgeItem);
        }

        public async Task<List<FridgeItemResponseDto>> GetFridgeItemResponsesByUserId(Guid userId)
        {
            var items = await _context.FridgeItems
                .Include(f => f.Ingredient)
                .Where(f => f.UserId == userId)
                .ToListAsync();

            return _mapper.Map<List<FridgeItemResponseDto>>(items);
        }

        public async Task<FridgeItemResponseDto?> UpdateFridgeItem(Guid userId, FridgeItemUpdateDto dto)
        {
            var item = await _context.FridgeItems
                .Include(f => f.Ingredient)
                .FirstOrDefaultAsync(f => f.Id == dto.Id && f.UserId == userId);

            if (item == null)
                return null;

            if (dto.Name != null) item.Name = dto.Name;
            if (dto.Quantity != null) item.Quantity = dto.Quantity;
            if (dto.Unit != null) item.Unit = dto.Unit;
            if (dto.ExpirationDate != null) item.ExpirationDate = dto.ExpirationDate;

            await _context.SaveChangesAsync();

            return _mapper.Map<FridgeItemResponseDto>(item);
        }
    }
}