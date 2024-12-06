using DOM.Exceptions;
using DOM.MealPlanning;
using DOM.Recipes.Ingredients;

namespace DAL.Groceries;

public interface IGroceryRepository
{
    Task<Result<GroceryList>> ReadGroceryListByIdNoTracking(Guid id);
    Task<Result<ItemQuantity>> ReadItemQuantityById(Guid id);
    Task<Result<GroceryList>> ReadGroceryListByAccountId(Guid accountId);
    Task<Result<GroceryItem>> ReadGroceryItemByNameAndMeasurement(string name, MeasurementType measurement);
    Task<Result<Unit>> CreateGroceryList(GroceryList groceryList);
    Task<Result<Unit>> UpdateGroceryList(GroceryList groceryList);
    Task<Result<Unit>> AddGroceryListItem(GroceryList groceryList, ItemQuantity newItem);
    Task<Result<Unit>> DeleteItemQuantity(Guid userId, Guid itemId);
}