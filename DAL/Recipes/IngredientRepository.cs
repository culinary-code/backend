﻿using DAL.EF;
using DOM.Recipes.Ingredients;

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
            throw new Exception($"No ingredient found with id {id}");
        }
        return ingredient;
    }

    public Ingredient ReadIngredientByName(string name)
    {
        Ingredient? ingredient = _ctx.Ingredients.FirstOrDefault(i => i.IngredientName == name);
        if (ingredient is null)
        {
            throw new Exception($"No ingredient found with name {name}");
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
}