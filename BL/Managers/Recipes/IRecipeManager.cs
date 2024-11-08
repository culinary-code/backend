using BL.DTOs.Recipes;

namespace BL.Managers.Recipes;

public interface IRecipeManager
{
    RecipeDto GetRecipeDtoById(string id);
    RecipeDto GetRecipeDtoByName(string name);
    ICollection<RecipeDto> GetRecipesCollectionByName(string name);
    RecipeDto? CreateRecipe(string name);
    ICollection<RecipeDto> CreateBatchRecipes(string input);
}