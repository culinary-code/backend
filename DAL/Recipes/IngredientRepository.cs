using System;
using System.Linq;
using DAL.EF;
using DOM.Recipes.Ingredients;
using DOM.Results;
using Microsoft.EntityFrameworkCore;

namespace DAL.Recipes;

public class IngredientRepository : IIngredientRepository
{
    private readonly CulinaryCodeDbContext _ctx;

    public IngredientRepository(CulinaryCodeDbContext ctx)
    {
        _ctx = ctx;
    }

    // used to create new planned meal, needs to be tracked
    public async Task<Result<Ingredient>> ReadIngredientById(Guid id)
    {
        Ingredient? ingredient = await _ctx.Ingredients.FindAsync(id);
        if (ingredient is null)
        {
            return Result<Ingredient>.Failure($"No ingredient found with id {id}", ResultFailureType.NotFound);
        }
        return Result<Ingredient>.Success(ingredient);
    }
    
    // used to update grocery list, needs to be tracked
    public async Task<Result<IngredientQuantity>> ReadIngredientQuantityById(Guid id)
    {
        IngredientQuantity? ingredientQuantity = await _ctx.IngredientQuantities.FindAsync(id);
        if (ingredientQuantity is null)
        {
            return Result<IngredientQuantity>.Failure($"No ingredientQuantity found with id {id}", ResultFailureType.NotFound);
        }
        return Result<IngredientQuantity>.Success(ingredientQuantity);
    }

    // used to create new recipes, needs to be tracked
    public async Task<Result<Ingredient>> ReadIngredientByNameAndMeasurementType(string name, MeasurementType measurementType)
    {
        Ingredient? ingredient = await _ctx.Ingredients.FirstOrDefaultAsync(i =>
            i.IngredientName == name && i.Measurement == measurementType);
        if (ingredient is null)
        {
            return Result<Ingredient>.Failure($"No ingredient found with name {name} and measurement type {measurementType}", ResultFailureType.NotFound);

        }
        return Result<Ingredient>.Success(ingredient);
    }

    public async Task<Result<Unit>> CreateIngredient(Ingredient ingredient)
    {
        await _ctx.Ingredients.AddAsync(ingredient);
        await _ctx.SaveChangesAsync();
        return Result<Unit>.Success(new Unit());
    }
    
    public async Task<Result<Unit>> DeleteIngredientQuantity(Guid userId, Guid ingredientQuantityId)
    {
        var ingredientQuantity = await _ctx.IngredientQuantities
            .Include(i => i.GroceryList)
                .ThenInclude(g => g.Account)
            .Include(i => i.PlannedMeal)
                .ThenInclude(p => p.NextWeekMealPlanner)
                .ThenInclude(n => n.Account)
            .FirstOrDefaultAsync(i => i.IngredientQuantityId == ingredientQuantityId);

        if (ingredientQuantity == null)
        {
            return Result<Unit>.Failure($"No ingredientQuantity found with id {ingredientQuantityId}", ResultFailureType.NotFound);
        }

        if (ingredientQuantity.GroceryList != null)
        {
            if (ingredientQuantity.GroceryList.Account!.AccountId != userId)
                return Result<Unit>.Failure(
                    "The ingredient quantity you are trying to remove belongs to another account",
                    ResultFailureType.Error);
            
            _ctx.IngredientQuantities.Remove(ingredientQuantity);
            await _ctx.SaveChangesAsync();
            return Result<Unit>.Success(new Unit());

        }
        if (ingredientQuantity.PlannedMeal != null)
        {
            if (ingredientQuantity.PlannedMeal.NextWeekMealPlanner!.Account!.AccountId != userId)
                return Result<Unit>.Failure(
                    "The ingredient quantity you are trying to remove belongs to another account",
                    ResultFailureType.Error);
            
            _ctx.IngredientQuantities.Remove(ingredientQuantity);
            await _ctx.SaveChangesAsync();
            return Result<Unit>.Success(new Unit());
        }

        // if it has no planned meal or grocery list, it has to be an ingredient quantity from a recipe
        return Result<Unit>.Failure("The ingredient quantity you are trying to remove belongs to a recipe", ResultFailureType.Error);
    }

    public async Task<Result<Unit>> DeleteIngredientQuantityFromGroup(Guid groupId, Guid ingredientQuantityId)
    {
        var ingredientQuantity = await _ctx.IngredientQuantities
            .Include(i => i.GroceryList)
            .ThenInclude(g => g.Group)
            .Include(gr => gr.PlannedMeal)
            .ThenInclude(p => p.NextWeekMealPlanner)
            .ThenInclude(n => n.Group)
            .FirstOrDefaultAsync(i => i.IngredientQuantityId == ingredientQuantityId);

        if (ingredientQuantity == null)
        {
            return Result<Unit>.Failure($"No ingredientQuantity found with id {ingredientQuantityId}", ResultFailureType.NotFound);
        }

        if (ingredientQuantity.GroceryList != null)
        {
            if (ingredientQuantity.GroceryList.Group!.GroupId != groupId)
                return Result<Unit>.Failure(
                    "The ingredient quantity you are trying to remove belongs to another account",
                    ResultFailureType.Error);
            
            _ctx.IngredientQuantities.Remove(ingredientQuantity);
            await _ctx.SaveChangesAsync();
            return Result<Unit>.Success(new Unit());

        }
        if (ingredientQuantity.PlannedMeal != null)
        {
            if (ingredientQuantity.PlannedMeal.NextWeekMealPlanner!.Group!.GroupId != groupId)
                return Result<Unit>.Failure(
                    "The ingredient quantity you are trying to remove belongs to another account",
                    ResultFailureType.Error);
            
            _ctx.IngredientQuantities.Remove(ingredientQuantity);
            await _ctx.SaveChangesAsync();
            return Result<Unit>.Success(new Unit());
        }

        // if it has no planned meal or grocery list, it has to be an ingredient quantity from a recipe
        return Result<Unit>.Failure("The ingredient quantity you are trying to remove belongs to a recipe", ResultFailureType.Error);

    }
}