using System.Collections.Generic;
using DOM.Recipes;

namespace DAL.Recipes;

public interface IRecipeRepository
{
    Recipe ReadRecipeById(int id);
    Recipe ReadRecipeByName(string name);
    ICollection<Recipe> ReadRecipesCollectionByName(string name);
}