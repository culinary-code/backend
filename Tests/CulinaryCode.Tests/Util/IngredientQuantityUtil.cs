using DOM.Recipes.Ingredients;

namespace CulinaryCode.Tests.Util;

public static class IngredientQuantityUtil
{
    public static IngredientQuantity CreateIngredientQuantity(float quantity, Ingredient ingredient)
    {
        return new IngredientQuantity()
        {
            Quantity = quantity,
            Ingredient = ingredient,
        };
    }
}