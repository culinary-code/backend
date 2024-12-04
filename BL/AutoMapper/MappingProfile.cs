using AutoMapper;
using BL.DTOs.Accounts;
using BL.DTOs.MealPlanning;
using BL.DTOs.Recipes;
using BL.DTOs.Recipes.Ingredients;
using DOM.Accounts;
using DOM.MealPlanning;
using DOM.Recipes;
using DOM.Recipes.Ingredients;

namespace BL.AutoMapper;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Account mappings
        CreateMap<Account, AccountDto>(); // From Account to AccountDto
        CreateMap<AccountDto, Account>(); // From AccountDto to Account

        // Preference mappings
        CreateMap<Preference, PreferenceDto>(); // From Preference to PreferenceDto
        CreateMap<PreferenceDto, Preference>(); // From PreferenceDto to Preference

        // Review mappings
        CreateMap<Review, ReviewDto>()
            .ForMember(dest => dest.ReviewerUsername, opt => opt.MapFrom(src => src.Account!.Name)); // From Review to ReviewDto
        CreateMap<ReviewDto, Review>(); // From ReviewDto to Review

        // GroceryList mappings
        CreateMap<GroceryList, GroceryListDto>(); // From GroceryList to GroceryListDto
        CreateMap<GroceryListDto, GroceryList>(); // From GroceryListDto to GroceryList

        // MealPlanner mappings
        CreateMap<MealPlanner, MealPlannerDto>(); // From MealPlanner to MealPlannerDto
        CreateMap<MealPlannerDto, MealPlanner>(); // From MealPlannerDto to MealPlanner

        // PlannedMeal mappings
        CreateMap<PlannedMeal, PlannedMealDto>(); // From PlannedMeal to PlannedMealDto
        CreateMap<PlannedMealDto, PlannedMeal>(); // From PlannedMealDto to PlannedMeal

        // Ingredient mappings
        CreateMap<Ingredient, IngredientDto>(); // From Ingredient to IngredientDto
        CreateMap<IngredientDto, Ingredient>(); // From IngredientDto to Ingredient
        
        // GroceryItem mappings
        CreateMap<GroceryItem, GroceryItemDto>(); // From GroceryItem to GroceryItemDto
        CreateMap<GroceryItemDto, GroceryItem>(); // From GroceryItemDto to GroceryItem

        // IngredientQuantity mappings
        CreateMap<IngredientQuantity, IngredientQuantityDto>() // from 
            .ForMember(dest => dest.RecipeName, opt => opt.MapFrom(src =>
                src.PlannedMeal != null && src.PlannedMeal.Recipe != null // not every ingredientquantity has a planned meal, could be linked to a recipe or a groceryList
                    ? src.PlannedMeal.Recipe.RecipeName 
                    : string.Empty)); // map the recipename from the planned meal to the dto recipeName property
        CreateMap<IngredientQuantityDto, IngredientQuantity>(); // From IngredientQuantityDto to IngredientQuantity

        // ItemQuantity mappings
        CreateMap<ItemQuantity, ItemQuantityDto>(); // From ItemQuantity to ItemQuantityDto
        CreateMap<ItemQuantityDto, ItemQuantity>(); // From ItemQuantityDto to ItemQuantity
        
        // FavoriteRecipe mappings
        CreateMap<FavoriteRecipe, FavoriteRecipeDto>(); // From FavoriteRecipe to FavoriteRecipeDto
        CreateMap<FavoriteRecipeDto, FavoriteRecipe>(); // From FavoriteRecipeDto to FavoriteRecipe

        // InstructionStep mappings
        CreateMap<InstructionStep, InstructionStepDto>(); // From InstructionStep to InstructionStepDto
        CreateMap<InstructionStepDto, InstructionStep>(); // From InstructionStepDto to InstructionStep
        
        // Recipe mappings
        CreateMap<Recipe, RecipeDto>(); // From Recipe to RecipeDto
        CreateMap<RecipeDto, Recipe>(); // From RecipeDto to Recipe
    }
}