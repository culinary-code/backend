using DOM.MealPlanning;
using DOM.Recipes.Ingredients;

namespace DAL.Groceries;

public interface IGroceryRepository
{
    Task<GroceryList> ReadGroceryListByIdNoTracking(Guid id);
    Task<ItemQuantity> ReadItemQuantityById(Guid id);
    Task<GroceryList> ReadGroceryListByAccountId(Guid accountId);
    Task<GroceryItem?> ReadPossibleGroceryItemByNameAndMeasurement(string name, MeasurementType measurement);
    Task CreateGroceryList(GroceryList groceryList);
    Task UpdateGroceryList(GroceryList groceryList);
    Task AddGroceryListItem(GroceryList groceryList, ItemQuantity newItem);
    Task DeleteItemQuantity(Guid userId, Guid itemId);
}