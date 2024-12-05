using System;
using System.Linq;
using DAL.EF;
using DOM.Exceptions;
using DOM.Recipes.Ingredients;
using Microsoft.EntityFrameworkCore;

namespace DAL.Recipes;

public class IngredientRepository : IIngredientRepository
{
    private readonly CulinaryCodeDbContext _ctx;

    public IngredientRepository(CulinaryCodeDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<Ingredient> ReadIngredientById(Guid id)
    {
        Ingredient? ingredient = await _ctx.Ingredients.FindAsync(id);
        if (ingredient is null)
        {
            throw new IngredientNotFoundException($"No ingredient found with id {id}");
        }
        return ingredient;
    }

    public async Task<IngredientQuantity> ReadIngredientQuantityById(Guid id)
    {
        IngredientQuantity? ingredientQuantity = await _ctx.IngredientQuantities.FindAsync(id);
        if (ingredientQuantity is null)
        {
            throw new IngredientQuantityNotFoundException($"No ingredientQuantity found with id {id}");
        }
        return ingredientQuantity;
    }

    public async Task<Ingredient> ReadIngredientByName(string name)
    {
        Ingredient? ingredient = await _ctx.Ingredients.FirstOrDefaultAsync(i => i.IngredientName == name);
        if (ingredient is null)
        {
            throw new IngredientNotFoundException($"No ingredient found with name {name}");
        }
        return ingredient;
    }

    public async Task<Ingredient?> ReadPossibleIngredientByNameAndMeasurement(string name, MeasurementType measurementType)
    {
        return await _ctx.Ingredients.FirstOrDefaultAsync(i => i.IngredientName == name && i.Measurement == measurementType);

    }

    public async Task<Ingredient> ReadIngredientByNameAndMeasurementType(string name, MeasurementType measurementType)
    {
        Ingredient? ingredient = await _ctx.Ingredients.FirstOrDefaultAsync(i =>
            i.IngredientName == name && i.Measurement == measurementType);
        if (ingredient is null)
        {
            throw new IngredientNotFoundException($"No ingredient found with name {name} and measurement type {measurementType}");
        }
        return ingredient;
    }

    public async Task CreateIngredient(Ingredient ingredient)
    {
        await _ctx.Ingredients.AddAsync(ingredient);
        await _ctx.SaveChangesAsync();
    }
    

    public async Task DeleteIngredientQuantity(Guid userId, Guid ingredientQuantityId)
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
            throw new IngredientQuantityNotFoundException($"No ingredientQuantity found with id {ingredientQuantityId}");
        }

        if (ingredientQuantity.GroceryList != null)
        {
            if (ingredientQuantity.GroceryList.Account!.AccountId == userId)
            {
                _ctx.IngredientQuantities.Remove(ingredientQuantity);
                await _ctx.SaveChangesAsync();
            }
            else
            {
                throw new AccountMismatchException(
                    "The ingredient quantity you are trying to remove belongs to another account");
            }
            
        }else if (ingredientQuantity.PlannedMeal != null)
        {
            if(ingredientQuantity.PlannedMeal.NextWeekMealPlanner!.Account!.AccountId == userId)
            {
                _ctx.IngredientQuantities.Remove(ingredientQuantity);
                await _ctx.SaveChangesAsync();
            }
            else
            {
                throw new AccountMismatchException(
                    "The ingredient quantity you are trying to remove belongs to another account");
            }
        }
        else
        {
            throw new IngredientQuantityNotFoundException(
                "The ingredient quantity you are trying to remove belongs to a recipe");
        }
        
    }
}