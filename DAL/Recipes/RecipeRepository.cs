using System;
using System.Collections.Generic;
using System.Linq;
using DAL.EF;
using DOM.Exceptions;
using DOM.Recipes;
using Microsoft.EntityFrameworkCore;

namespace DAL.Recipes;

public class RecipeRepository : IRecipeRepository
{
    private readonly CulinaryCodeDbContext _ctx;

    public RecipeRepository(CulinaryCodeDbContext ctx)
    {
        _ctx = ctx;
    }

    public Recipe ReadRecipeById(Guid id)
    {
        Recipe? recipe = _ctx.Recipes
            .Include(r => r.Ingredients)
            .ThenInclude(i => i.Ingredient)
            .Include(r => r.Instructions)
            .Include(r => r.Reviews)
            .Include(r => r.Preferences)
            .FirstOrDefault(r => r.RecipeId == id );
        if (recipe is null)
        {
            throw new RecipeNotFoundException($"No recipe found with id {id}");
        }
        return recipe;
    }

    public Recipe ReadRecipeByName(string name)
    {
        string lowerName = name.ToLower();
        Recipe? recipe = _ctx.Recipes
            .FirstOrDefault(r => r.RecipeName.ToLower().Contains(lowerName));
        if (recipe is null)
        {
            throw new RecipeNotFoundException($"No recipe found with name {name}");
        }
        return recipe;
    }
    
    public ICollection<Recipe> ReadRecipesCollectionByName(string name)
    {
        string lowerName = name.ToLower();
        ICollection<Recipe> recipes = _ctx.Recipes.Where(r => r.RecipeName.ToLower().Contains(lowerName)).ToList();
        if (recipes.Count <= 0)
        {
            throw new RecipeNotFoundException($"No recipes found with name {name}");
        }
        return recipes;
    }
}