using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DOM.Recipes;

namespace DAL.Recipes;

public interface IRecipeRepository
{
    Recipe ReadRecipeById(Guid id);
    Recipe ReadRecipeByName(string name);
    ICollection<Recipe> ReadRecipesCollectionByName(string name);
    Task<ICollection<Recipe>> GetFilteredRecipesAsync(string recipeName, Difficulty difficulty, RecipeType recipeType, int cooktime, List<string> ingredients);
    Task<int> GetRecipeCountAsync();
    void CreateRecipe(Recipe recipe);
    Task CreateRecipeAsync(Recipe recipe);
    void UpdateRecipe(Recipe recipe);
    Task DeleteUnusedRecipesAsync();
}