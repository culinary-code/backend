using DOM.Accounts;
using DOM.MealPlanning;
using DOM.Recipes.Ingredients;

namespace CulinaryCode.Tests.Util;

public static class GroceryUtil
{
    public static GroceryList CreateGroceryList()
    {
        return new GroceryList
        {
            GroceryListId = Guid.NewGuid(),
            Account = new Account
            {
                AccountId = Guid.NewGuid(),
            },
            Ingredients = new List<IngredientQuantity>(),
            Items = new List<ItemQuantity>()
        };
    }

    public static ItemQuantity CreateItemQuantity()
    {
        return new ItemQuantity
        {
            ItemQuantityId = Guid.NewGuid(),
            Quantity = 2,
            GroceryItem = CreateGroceryItem()
        };
    }

    public static GroceryItem CreateGroceryItem()
    {
        return new GroceryItem
        {
            GroceryItemId = Guid.NewGuid(),
            GroceryItemName = "Apples",
            Measurement = MeasurementType.Kilogram
        };
    }
}