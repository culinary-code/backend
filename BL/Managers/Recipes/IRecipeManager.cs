using BL.DTOs.Accounts;
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
    Task<int> GetAmountOfRecipesAsync();
    Task<RecipeDto?> CreateRecipe(RecipeFilterDto request, List<PreferenceDto> preferences);
    Task<RecipeDto?> CreateRecipeAsync(RecipeFilterDto request, List<PreferenceDto> preferences);
    Task<ICollection<RecipeDto>> CreateBatchRecipes(string input);
    Task CreateBatchRandomRecipes(int amount, List<PreferenceDto>? preferences);
    Task RemoveUnusedRecipesAsync();
}