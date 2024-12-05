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

    // used create new favorite recipes and planned meals, needs to be tracked
    public async Task<Recipe> ReadRecipeById(Guid id)
    {
        Recipe? recipe = await _ctx.Recipes
            .Include(r => r.Ingredients)
            .ThenInclude(i => i.Ingredient)
            .Include(r => r.Instructions)
            .Include(r => r.Reviews)
            .ThenInclude(r => r.Account)
            .Include(r => r.Preferences)
            .FirstOrDefaultAsync(r => r.RecipeId == id);
        if (recipe is null)
        {
            throw new RecipeNotFoundException($"No recipe found with id {id}");
        }

        return recipe;
    }
    
    // used to return dto, doesn't need to be tracked
    public async Task<Recipe> ReadRecipeWithRelatedInformationByIdNoTracking(Guid id)
    {
        Recipe? recipe = await _ctx.Recipes
            .Include(r => r.Ingredients)
            .ThenInclude(i => i.Ingredient)
            .Include(r => r.Instructions)
            .Include(r => r.Reviews)
            .ThenInclude(r => r.Account)
            .Include(r => r.Preferences)
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync(r => r.RecipeId == id);
        if (recipe is null)
        {
            throw new RecipeNotFoundException($"No recipe found with id {id}");
        }

        return recipe;
    }
    
    // used to create new reviews, needs to be tracked
    public async Task<Recipe> ReadRecipeWithReviewsById(Guid id)
    {
        Recipe? recipe = await _ctx.Recipes
            .Include(r => r.Reviews)
            .ThenInclude(r => r.Account)
            .FirstOrDefaultAsync(r => r.RecipeId == id);
        if (recipe is null)
        {
            throw new RecipeNotFoundException($"No recipe found with id {id}");
        }

        return recipe;
    }

    // used to return a dto, doesn't need to be tracked
    public async Task<Recipe> ReadRecipeByNameNoTracking(string name)
    {
        string lowerName = name.ToLower();
        Recipe? recipe = await _ctx.Recipes
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync(r => r.RecipeName.ToLower().Contains(lowerName));
        if (recipe is null)
        {
            throw new RecipeNotFoundException($"No recipe found with name {name}");
        }

        return recipe;
    }

    // used to return a dto, doesn't need to be tracked
    public async Task<ICollection<Recipe>> ReadRecipesCollectionByNameNoTracking(string name)
    {
        string lowerName = name.ToLower();
        ICollection<Recipe> recipes = await _ctx.Recipes
            .Where(r => r.RecipeName.ToLower().Contains(lowerName))
            .AsNoTrackingWithIdentityResolution()
            .ToListAsync();
        if (recipes.Count <= 0)
        {
            throw new RecipeNotFoundException($"No recipes found with name {name}");
        }

        return recipes;
    }

    // used to return a dto, doesn't need to be tracked
    public async Task<ICollection<Recipe>> GetFilteredRecipesNoTracking(string recipeName, Difficulty difficulty,
        RecipeType recipeType, int cooktime, List<String> ingredientStrings)
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
            .AsNoTrackingWithIdentityResolution()
            .ToListAsync();

        return recipes;
    }

    public async Task<int> GetRecipeCount()
    {
        return await _ctx.Recipes.CountAsync();
    }

    public async Task CreateRecipe(Recipe recipe)
    {
        await _ctx.Recipes.AddAsync(recipe);
        await _ctx.SaveChangesAsync();
    }

    public async Task UpdateRecipe(Recipe recipe)
    {
        _ctx.Recipes.Update(recipe);
        await _ctx.SaveChangesAsync();
    }

    public async Task DeleteUnusedRecipes()
    {
        var thresholdDate = DateTime.UtcNow.AddDays(-31);

        // Delete recipes with no favorites and older than 31 days
        var recipesToDelete = _ctx.Recipes
            .Where(r => r.LastUsedAt < thresholdDate && !r.FavoriteRecipes.Any());

        // Remove them in bulk
        _ctx.Recipes.RemoveRange(recipesToDelete);

        // Save changes
        await _ctx.SaveChangesAsync();
    }
}