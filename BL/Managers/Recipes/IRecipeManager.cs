using BL.DTOs.Recipes;
using DOM.Recipes;

namespace BL.Managers.Recipes;

public interface IRecipeManager
{
    RecipeDto GetRecipeDtoById(string id);
    RecipeDto GetRecipeDtoByName(string name);
    ICollection<RecipeDto> GetRecipesCollectionByName(string name);
    Task<ICollection<RecipeDto>> GetFilteredRecipeCollection(string recipeName, Difficulty difficulty,
        RecipeType recipeType, int cooktime, List<string> ingredients);
    RecipeDto? CreateRecipe(RecipeFilterDto request);
    ICollection<RecipeDto> CreateBatchRecipes(string input);
    void CreateBatchRandomRecipes(int amount);
}