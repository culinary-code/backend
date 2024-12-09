using BL.DTOs.Accounts;
using BL.DTOs.Recipes;
using DOM.Exceptions;
using DOM.Recipes;

namespace BL.Managers.Recipes;

public interface IRecipeManager
{
    Task<Result<RecipeDto>> GetRecipeDtoById(string id);
    Task<Result<RecipeDto>> GetRecipeDtoByName(string name);
    Task<Result<ICollection<RecipeDto>>> GetRecipesCollectionByName(string name);
    Task<Result<ICollection<RecipeDto>>> GetFilteredRecipeCollection(string recipeName, Difficulty difficulty,
        RecipeType recipeType, int cooktime, List<string> ingredients);
    Task<Result<int>> GetAmountOfRecipes();
    Task<Result<RecipeDto?>> CreateRecipe(RecipeFilterDto request, List<PreferenceDto> preferences);
    Task<Result<ICollection<RecipeDto>>> CreateBatchRecipes(string input);
    Task<Result<Unit>> CreateBatchRandomRecipes(int amount, List<PreferenceDto>? preferences);
    Task<Result<Unit>> RemoveUnusedRecipes();
    Task<ICollection<RecipeSuggestionDto>> CreateRecipeSuggestions(RecipeFilterDto request, List<PreferenceDto> preferences, int amount = 5);
}