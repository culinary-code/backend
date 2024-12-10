using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DAL.EF;
using DOM.Recipes;
using DOM.Results;
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
    public async Task<Result<Recipe>> ReadRecipeById(Guid id)
    {
        Recipe? recipe = await _ctx.Recipes
            .FirstOrDefaultAsync(r => r.RecipeId == id);
        if (recipe is null)
        {
            return Result<Recipe>.Failure($"No recipe found with id {id}", ResultFailureType.NotFound);
        }

        return Result<Recipe>.Success(recipe);
    }
    
    // used to return dto, doesn't need to be tracked
    public async Task<Result<Recipe>> ReadRecipeWithRelatedInformationByIdNoTracking(Guid id)
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
            return Result<Recipe>.Failure($"No recipe found with id {id}", ResultFailureType.NotFound);
        }

        return Result<Recipe>.Success(recipe);
    }
    
    // used to create new reviews, needs to be tracked
    public async Task<Result<Recipe>> ReadRecipeWithReviewsById(Guid id)
    {
        Recipe? recipe = await _ctx.Recipes
            .Include(r => r.Reviews)
            .ThenInclude(r => r.Account)
            .FirstOrDefaultAsync(r => r.RecipeId == id);
        if (recipe is null)
        {
            return Result<Recipe>.Failure($"No recipe found with id {id}", ResultFailureType.NotFound);
        }

        return Result<Recipe>.Success(recipe);
    }

    // used to return a dto, doesn't need to be tracked
    public async Task<Result<Recipe>> ReadRecipeByNameNoTracking(string name)
    {
        string lowerName = name.ToLower();
        Recipe? recipe = await _ctx.Recipes
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync(r => r.RecipeName.ToLower().Contains(lowerName));
        if (recipe is null)
        {
            return Result<Recipe>.Failure($"No recipe found with name {name}", ResultFailureType.NotFound);
        }

        return Result<Recipe>.Success(recipe);
    }

    // used to return a dto, doesn't need to be tracked
    public async Task<Result<ICollection<Recipe>>> ReadRecipesCollectionByNameNoTracking(string name)
    {
        string lowerName = name.ToLower();
        ICollection<Recipe> recipes = await _ctx.Recipes
            .Where(r => r.RecipeName.ToLower().Contains(lowerName))
            .AsNoTrackingWithIdentityResolution()
            .ToListAsync();
        if (recipes.Count == 0)
        {
            return Result<ICollection<Recipe>>.Failure($"No recipes found with name {name}", ResultFailureType.NotFound);
        }

        return Result<ICollection<Recipe>>.Success(recipes);
    }

    // used to return a dto, doesn't need to be tracked
    public async Task<Result<ICollection<Recipe>>> GetFilteredRecipesNoTracking(string recipeName, Difficulty difficulty,
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
        
        if (recipes.Count == 0)
        {
            return Result<ICollection<Recipe>>.Failure($"No recipes found with these filters", ResultFailureType.NotFound);
        }

        return Result<ICollection<Recipe>>.Success(recipes);
    }

    public async Task<Result<int>> GetRecipeCount()
    {
        var count = await _ctx.Recipes.CountAsync();
        return Result<int>.Success(count);
    }

    public async Task<Result<Unit>> CreateRecipe(Recipe recipe)
    {
        await _ctx.Recipes.AddAsync(recipe);
        await _ctx.SaveChangesAsync();
        return Result<Unit>.Success(new Unit());
    }

    public async Task<Result<Unit>> UpdateRecipe(Recipe recipe)
    {
        _ctx.Recipes.Update(recipe);
        await _ctx.SaveChangesAsync();
        return Result<Unit>.Success(new Unit());
    }

    public async Task<Result<Unit>> DeleteUnusedRecipes()
    {
        var thresholdDate = DateTime.UtcNow.AddDays(-31);

        // Delete recipes with no favorites and older than 31 days
        var recipesToDelete = _ctx.Recipes
            .Where(r => r.LastUsedAt < thresholdDate && !r.FavoriteRecipes.Any());

        // Remove them in bulk
        _ctx.Recipes.RemoveRange(recipesToDelete);

        // Save changes
        await _ctx.SaveChangesAsync();
        return Result<Unit>.Success(new Unit());
    }
}