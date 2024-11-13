using BL.DTOs.MealPlanning;
using DOM.MealPlanning;

namespace BL.Managers.Groceries;

public interface IGroceryManager
{
    GroceryListDto CreateGroceryList(Guid accountId);
}