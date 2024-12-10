using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DOM.Recipes;
using DOM.Results;

namespace DAL.Recipes;

public interface IRecipeRepository
{
    Task<Result<Recipe>> ReadRecipeById(Guid id);
    Task<Result<Recipe>> ReadRecipeWithRelatedInformationByIdNoTracking(Guid id);
    Task<Result<Recipe>> ReadRecipeWithReviewsById(Guid id);
    Task<Result<Recipe>> ReadRecipeByNameNoTracking(string name);
    Task<Result<ICollection<Recipe>>> ReadRecipesCollectionByNameNoTracking(string name);
    Task<Result<ICollection<Recipe>>> GetFilteredRecipesNoTracking(string recipeName, Difficulty difficulty, RecipeType recipeType, int cooktime, List<string> ingredients);
    Task<Result<int>> GetRecipeCount();
    Task<Result<Unit>> CreateRecipe(Recipe recipe);
    Task<Result<Unit>> UpdateRecipe(Recipe recipe);
    Task<Result<Unit>> DeleteUnusedRecipes();
}