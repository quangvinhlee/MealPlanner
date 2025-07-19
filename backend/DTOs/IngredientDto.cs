using System;
using System.Collections.Generic;

namespace MealPlannerApp.DTOs
{
    public class IngredientDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public List<FridgeItemResponseDto> FridgeItems { get; set; } = new List<FridgeItemResponseDto>();
        public List<RecipeResponseDto> Recipes { get; set; } = new List<RecipeResponseDto>();
    }


}