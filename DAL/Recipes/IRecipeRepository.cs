using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DOM.Recipes;

namespace DAL.Recipes;

public interface IRecipeRepository
{
    Task<Recipe> ReadRecipeById(Guid id);
    Task<Recipe> ReadRecipeByName(string name);
    Task<ICollection<Recipe>> ReadRecipesCollectionByName(string name);
    Task<ICollection<Recipe>> GetFilteredRecipesAsync(string recipeName, Difficulty difficulty, RecipeType recipeType, int cooktime, List<string> ingredients);
    Task<int> GetRecipeCountAsync();
    Task CreateRecipe(Recipe recipe);
    Task CreateRecipeAsync(Recipe recipe);
    Task UpdateRecipe(Recipe recipe);
    Task DeleteUnusedRecipesAsync();
}