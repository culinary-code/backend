using System.Collections.Generic;
using DAL;
using DAL.Recipes;
using DOM.Recipes;

namespace BL.Recipes;

public class RecipeManager : IRecipeManager
{
    private readonly IRecipeRepository _repository;

    public RecipeManager(IRecipeRepository repository)
    {
        _repository = repository;
    }

    public Recipe GetRecipeById(int id)
    {
        return _repository.ReadRecipeById(id);
    }

    public Recipe GetRecipeByName(string name)
    {
        return _repository.ReadRecipeByName(name);
    }
    
    public ICollection<Recipe> GetRecipesCollectionByName(string name)
    {
        return _repository.ReadRecipesCollectionByName(name);
    }
}