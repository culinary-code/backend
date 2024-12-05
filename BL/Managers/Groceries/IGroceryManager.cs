using BL.DTOs.MealPlanning;
using BL.DTOs.Recipes.Ingredients;
using DOM.MealPlanning;

namespace BL.Managers.Groceries;

public interface IGroceryManager
{
    Task CreateNewGroceryList(GroceryList groceryList);
    Task<GroceryListDto> GetGroceryList(string id);
    Task<GroceryListDto> GetGroceryListWithNextWeek(Guid id);
    Task<GroceryListDto> GetGroceryListByAccountId(string accountId);
    Task AddItemToGroceryList(Guid userId, ItemQuantityDto addItem);
    Task RemoveItemFromGroceryList(Guid userId, ItemQuantityDto removeItem);
}