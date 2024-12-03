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

    public Ingredient ReadIngredientById(Guid id)
    {
        Ingredient? ingredient = _ctx.Ingredients.Find(id);
        if (ingredient is null)
        {
            throw new IngredientNotFoundException($"No ingredient found with id {id}");
        }
        return ingredient;
    }

    public IngredientQuantity ReadIngredientQuantityById(Guid id)
    {
        IngredientQuantity? ingredientQuantity = _ctx.IngredientQuantities.Find(id);
        if (ingredientQuantity is null)
        {
            throw new IngredientQuantityNotFoundException($"No ingredientQuantity found with id {id}");
        }
        return ingredientQuantity;
    }

    public Ingredient ReadIngredientByName(string name)
    {
        Ingredient? ingredient = _ctx.Ingredients.FirstOrDefault(i => i.IngredientName == name);
        if (ingredient is null)
        {
            throw new IngredientNotFoundException($"No ingredient found with name {name}");
        }
        return ingredient;
    }

    public Ingredient? ReadPossibleIngredientByName(string name)
    {
        return _ctx.Ingredients.FirstOrDefault(i => i.IngredientName == name);

    }

    public Ingredient ReadIngredientByNameAndMeasurementType(string name, MeasurementType measurementType)
    {
        Ingredient? ingredient = _ctx.Ingredients.FirstOrDefault(i =>
            i.IngredientName == name && i.Measurement == measurementType);
        if (ingredient is null)
        {
            throw new IngredientNotFoundException($"No ingredient found with name {name} and measurement type {measurementType}");
        }
        return ingredient;
    }

    public void CreateIngredient(Ingredient ingredient)
    {
        _ctx.Ingredients.Add(ingredient);
        _ctx.SaveChanges();
    }

    public void UpdateIngredient(Ingredient ingredient)
    {
        _ctx.Ingredients.Update(ingredient);
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