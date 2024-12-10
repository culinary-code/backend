using BL.DTOs.MealPlanning;
using BL.DTOs.Recipes.Ingredients;
using DOM.MealPlanning;
using DOM.Results;

namespace BL.Managers.Groceries;

public interface IGroceryManager
{
    Task<Result<GroceryListDto>> GetGroceryListWithNextWeek(Guid id);
    Task<Result<GroceryListDto>> GetGroceryListByAccountId(string accountId);
    Task<Result<Unit>> AddItemToGroceryList(Guid userId, ItemQuantityDto addItem);
    Task<Result<Unit>> RemoveItemFromGroceryList(Guid userId, ItemQuantityDto removeItem);
}