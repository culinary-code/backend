using DOM.Recipes.Ingredients;

namespace CulinaryCode.Tests.Util;

public static class IngredientUtil
{
    public static Ingredient CreateIngredient(string ingredientName = "test ingredient", MeasurementType measurementType = MeasurementType.Piece)
    {
        return new Ingredient()
        {
            IngredientName = ingredientName,
            Measurement = measurementType
        };
    }
}