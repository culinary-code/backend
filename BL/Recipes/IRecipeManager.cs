using System;
using System.Collections.Generic;
using DOM.Recipes;

namespace BL.Recipes;

public interface IRecipeManager
{
    Recipe GetRecipeById(int id);
    Recipe GetRecipeByName(string name);
    ICollection<Recipe> GetRecipesCollectionByName(string name);
}