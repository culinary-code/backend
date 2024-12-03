using System;
using DOM.Recipes.Ingredients;

namespace DAL.Recipes;

public interface IIngredientRepository
{
    public Ingredient ReadIngredientById(Guid id);
    public IngredientQuantity ReadIngredientQuantityById(Guid id);
    public Ingredient ReadIngredientByName(string name);
    public Ingredient? ReadPossibleIngredientByNameAndMeasurement(string name, MeasurementType measurement);
    public Ingredient ReadIngredientByNameAndMeasurementType(string name, MeasurementType measurementType);
    public void CreateIngredient(Ingredient ingredient);
    public void UpdateIngredient(Ingredient ingredient);
    public Task DeleteIngredientQuantity(Guid userId, Guid ingredientQuantityId);
}