using System;
using DOM.Recipes;

namespace DAL;

public interface IRecipeRepository
{
    Recipe? GetRecipeById(int id);
}