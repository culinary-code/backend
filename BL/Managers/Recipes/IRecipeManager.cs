﻿using BL.DTOs.Accounts;
using BL.DTOs.Recipes;
using DOM.Recipes;

namespace BL.Managers.Recipes;

public interface IRecipeManager
{
    Task<RecipeDto> GetRecipeDtoById(string id);
    Task<RecipeDto> GetRecipeDtoByName(string name);
    Task<ICollection<RecipeDto>> GetRecipesCollectionByName(string name);
    Task<ICollection<RecipeDto>> GetFilteredRecipeCollection(string recipeName, Difficulty difficulty,
        RecipeType recipeType, int cooktime, List<string> ingredients);
    Task<int> GetAmountOfRecipes();
    Task<RecipeDto?> CreateRecipe(RecipeFilterDto request, List<PreferenceDto> preferences);
    Task<ICollection<RecipeSuggestionDto>> CreateRecipeSuggestions(RecipeFilterDto request, List<PreferenceDto> preferences, int amount = 5);
    Task<ICollection<RecipeDto>> CreateBatchRecipes(string input);
    Task CreateBatchRandomRecipes(int amount, List<PreferenceDto>? preferences);
    Task RemoveUnusedRecipes();
}