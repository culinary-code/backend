using BL.DTOs.MealPlanning;
using BL.DTOs.Recipes.Ingredients;
using DOM.MealPlanning;

namespace BL.Managers.Groceries;

public interface IGroceryManager
{
    //GroceryListDto CreateGroceryList(Guid accountId);
    void CreateNewGroceryList(GroceryList groceryList);
    GroceryListDto GetGroceryList(string id);
    GroceryListDto GetGroceryListByAccountId(string accountId);
    void AddItemToGroceryList(Guid groceryListId, ItemQuantityDto addItem);
    GroceryListDto? UpdateGroceryList(Guid accountId, GroceryListDto groceryList);

}