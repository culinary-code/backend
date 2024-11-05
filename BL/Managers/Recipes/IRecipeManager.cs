using System.Collections.Generic;
using BL.DTOs.Recipes;
using DOM.Recipes;

namespace BL.Managers.Recipes;

public interface IRecipeManager
{
    RecipeDto GetRecipeDtoById(string id);
    Recipe GetRecipeByName(string name);
    ICollection<Recipe> GetRecipesCollectionByName(string name);
}