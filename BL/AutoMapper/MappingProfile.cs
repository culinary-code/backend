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
        CreateMap<Review, ReviewDto>(); // From Review to ReviewDto
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

        // IngredientQuantity mappings
        CreateMap<IngredientQuantity, IngredientQuantityDto>(); // From IngredientQuantity to IngredientQuantityDto
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