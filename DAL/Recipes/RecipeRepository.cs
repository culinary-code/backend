using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            .ThenInclude(r => r.Account)
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

    public async Task<ICollection<Recipe>> GetFilteredRecipesAsync(string recipeName, Difficulty difficulty, RecipeType recipeType, int cooktime, List<String> ingredientStrings)
    {
        var query = _ctx.Recipes.AsQueryable();

        // Apply filters conditionally
        if (!string.IsNullOrEmpty(recipeName))
        {
            string lowerName = recipeName.ToLower();
            query = query.Where(r => r.RecipeName.ToLower().Contains(lowerName));
        }

        if (difficulty != Difficulty.NotAvailable) // Assuming `None` means no filtering
        {
            query = query.Where(r => r.Difficulty == difficulty);
        }

        if (recipeType != RecipeType.NotAvailable) // Assuming `None` means no filtering
        {
            query = query.Where(r => r.RecipeType == recipeType);
        }

        if (cooktime > 0)
        {
            query = query.Where(r => r.CookingTime <= cooktime);
        }

        if (ingredientStrings.Any())
        {
            // Filter based on whether all ingredients in ingredientStrings exist in the recipe
            query = query.Where(r => ingredientStrings.All(ingredient =>
                r.Ingredients.Any(iq => iq.Ingredient != null && 
                                        (iq.Ingredient.IngredientName.ToLower().StartsWith(ingredient.ToLower()) || 
                                         iq.Ingredient.IngredientName.ToLower().Contains(" " + ingredient.ToLower()))
                )
            ));
        }
        
        // Execute the query and return the results
        var recipes = await query
            .ToListAsync();

        return recipes;
    }

    public Task<int> GetRecipeCountAsync()
    {
        return _ctx.Recipes.CountAsync();
    }

    public void CreateRecipe(Recipe recipe)
    {
        _ctx.Recipes.Add(recipe);
        _ctx.SaveChanges();
    }
    
    public async Task CreateRecipeAsync(Recipe recipe)
    {
        await _ctx.Recipes.AddAsync(recipe);
        await _ctx.SaveChangesAsync();
    }

    public void UpdateRecipe(Recipe recipe)
    {
        _ctx.Recipes.Update(recipe);
        _ctx.SaveChanges();
    }
}