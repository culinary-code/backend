using DOM.MealPlanning;
using DOM.Recipes.Ingredients;

namespace DAL.Groceries;

public interface IGroceryRepository
{
    GroceryList ReadGroceryListById(Guid id);
    GroceryList ReadGroceryListByAccountId(Guid accountId);
    void CreateGroceryList(GroceryList groceryList);
    void UpdateGroceryList(GroceryList groceryList);
    void AddGroceryListItem(GroceryList groceryList, ItemQuantity newItem);
    Task DeleteItemFromGroceryList(Guid groceryListId, Guid itemId);
}