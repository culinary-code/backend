﻿using BL.DTOs.MealPlanning;
using BL.DTOs.Recipes.Ingredients;
using DOM.MealPlanning;

namespace BL.Managers.Groceries;

public interface IGroceryManager
{
    void CreateNewGroceryList(GroceryList groceryList);
    GroceryListDto GetGroceryList(string id);
    GroceryListDto getGroceryListWithNextWeek(Guid id);
    GroceryListDto GetGroceryListByAccountId(string accountId);
    void AddItemToGroceryList(Guid groceryListId, ItemQuantityDto addItem);
    Task RemoveItemFromGroceryList(Guid groceryListId, Guid itemId);
}