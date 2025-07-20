using AutoMapper;
using MealPlannerApp.Models;
using MealPlannerApp.DTOs;
using Newtonsoft.Json.Linq;

namespace MealPlannerApp.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Entity to DTO mappings
            CreateMap<FridgeItems, FridgeItemResponseDto>();
            CreateMap<User, UserResponseDto>();
            CreateMap<Ingredient, IngredientDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));
            CreateMap<Recipes, RecipeResponseDto>();

            // DTO to Entity mappings
            CreateMap<FridgeItemCreateDto, FridgeItems>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.UserId, opt => opt.Ignore());

            CreateMap<FridgeItemUpdateDto, FridgeItems>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore());

            // Spoonacular JSON to DTO mappings
            CreateMap<JObject, SpoonacularIngredientDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => GetIntValue(src, "id")))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => GetStringValue(src, "name")))
                .ForMember(dest => dest.Original, opt => opt.MapFrom(src => GetStringValue(src, "original")))
                .ForMember(dest => dest.Image, opt => opt.MapFrom(src => GetStringValue(src, "image")))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => GetDoubleValue(src, "amount")))
                .ForMember(dest => dest.Unit, opt => opt.MapFrom(src => GetStringValue(src, "unit")));

            CreateMap<JObject, RecipeResponseDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => GetIntValue(src, "id")))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => GetStringValue(src, "title")))
                .ForMember(dest => dest.Image, opt => opt.MapFrom(src => GetStringValue(src, "image")))
                .ForMember(dest => dest.UsedIngredientCount, opt => opt.MapFrom(src => GetIntValue(src, "usedIngredientCount")))
                .ForMember(dest => dest.MissedIngredientCount, opt => opt.MapFrom(src => GetIntValue(src, "missedIngredientCount")))
                .ForMember(dest => dest.Likes, opt => opt.MapFrom(src => GetIntValue(src, "likes")))
                .ForMember(dest => dest.UsedIngredients, opt => opt.MapFrom((src, dest, destMember, context) => MapIngredientArray(src, "usedIngredients", context.Mapper)))
                .ForMember(dest => dest.MissedIngredients, opt => opt.MapFrom((src, dest, destMember, context) => MapIngredientArray(src, "missedIngredients", context.Mapper)))
                .ForMember(dest => dest.UnusedIngredients, opt => opt.MapFrom((src, dest, destMember, context) => MapIngredientArray(src, "unusedIngredients", context.Mapper)));
        }

        private static int GetIntValue(JObject obj, string propertyName)
        {
            return (int?)obj[propertyName] ?? 0;
        }

        private static string GetStringValue(JObject obj, string propertyName)
        {
            return (string?)obj[propertyName] ?? string.Empty;
        }

        private static double GetDoubleValue(JObject obj, string propertyName)
        {
            return (double?)obj[propertyName] ?? 0.0;
        }

        // Updated method that uses IRuntimeMapper instead of IMapper
        private static List<SpoonacularIngredientDto> MapIngredientArray(JObject obj, string propertyName, IRuntimeMapper mapper)
        {
            var ingredientsArray = obj[propertyName] as JArray;
            if (ingredientsArray == null) return new List<SpoonacularIngredientDto>();

            // Use the provided mapper instance to convert each JObject to SpoonacularIngredientDto
            return ingredientsArray
                .OfType<JObject>()
                .Select(ingredientObj => mapper.Map<SpoonacularIngredientDto>(ingredientObj))
                .ToList();
        }
    }
}