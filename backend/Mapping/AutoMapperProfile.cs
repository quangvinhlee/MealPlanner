using AutoMapper;
using MealPlannerApp.Models;
using MealPlannerApp.DTOs;

namespace MealPlannerApp.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // FridgeItems to FridgeItemResponseDto
            CreateMap<FridgeItems, FridgeItemResponseDto>();

            // User to UserResponseDto
            CreateMap<User, UserResponseDto>();

            // Ingredient to IngredientDto
            CreateMap<Ingredient, IngredientDto>();

            // Recipe to RecipeResponseDto
            CreateMap<Recipes, RecipeResponseDto>();

            // FridgeItemCreateDto to FridgeItems
            CreateMap<FridgeItemCreateDto, FridgeItems>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.UserId, opt => opt.Ignore()); // Will be set manually

            // FridgeItemUpdateDto to FridgeItems
            CreateMap<FridgeItemUpdateDto, FridgeItems>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore());
        }
    }
}