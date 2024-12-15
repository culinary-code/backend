﻿using System;
using DOM.Recipes.Ingredients;
using DOM.Results;

namespace DAL.Recipes;

public interface IIngredientRepository
{
    Task<Result<Ingredient>> ReadIngredientById(Guid id);
    Task<Result<IngredientQuantity>> ReadIngredientQuantityById(Guid id);
    Task<Result<Ingredient>> ReadIngredientByNameAndMeasurementType(string name, MeasurementType measurementType);
    Task<Result<Unit>> CreateIngredient(Ingredient ingredient);
    Task<Result<Unit>> DeleteIngredientQuantity(Guid userId, Guid ingredientQuantityId);
    Task<Result<Unit>> DeleteIngredientQuantityFromGroup(Guid groupId, Guid ingredientQuantityId);
}