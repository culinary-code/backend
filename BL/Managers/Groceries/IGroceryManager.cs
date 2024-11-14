using BL.DTOs.MealPlanning;
using BL.DTOs.Recipes.Ingredients;
using DOM.MealPlanning;
using DOM.Recipes.Ingredients;

namespace BL.Managers.Groceries;

public interface IGroceryManager
{
    GroceryListDto GetGroceryListById(string id);
    GroceryListDto CreateGroceryList(Guid accountId);
    void AddItemToGroceryList(Guid groceryListId, ItemQuantityDto addItemDto);

}