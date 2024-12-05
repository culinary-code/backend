using System;
using DOM.Recipes.Ingredients;

namespace DAL.Recipes;

public interface IIngredientRepository
{
    Task<Ingredient> ReadIngredientById(Guid id);
    Task<IngredientQuantity> ReadIngredientQuantityById(Guid id);
    Task<Ingredient?> ReadPossibleIngredientByNameAndMeasurement(string name, MeasurementType measurement);
    Task<Ingredient> ReadIngredientByNameAndMeasurementType(string name, MeasurementType measurementType);
    Task CreateIngredient(Ingredient ingredient);
    Task DeleteIngredientQuantity(Guid userId, Guid ingredientQuantityId);
}