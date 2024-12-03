using DOM.MealPlanning;
using DOM.Recipes.Ingredients;

namespace DAL.Groceries;

public interface IGroceryRepository
{
    GroceryList ReadGroceryListById(Guid id);
    ItemQuantity ReadItemQuantityById(Guid id);
    GroceryList ReadGroceryListByAccountId(Guid accountId);
    GroceryItem? ReadPossibleGroceryItemByName(string name);
    void CreateGroceryList(GroceryList groceryList);
    void UpdateGroceryList(GroceryList groceryList);
    void AddGroceryListItem(GroceryList groceryList, ItemQuantity newItem);
    Task DeleteItemQuantity(Guid userId, Guid itemId);
}