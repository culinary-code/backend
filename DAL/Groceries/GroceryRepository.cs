using DAL.EF;
using DOM.Exceptions;
using DOM.MealPlanning;
using DOM.Recipes.Ingredients;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DAL.Groceries;

public class GroceryRepository : IGroceryRepository
{
    private readonly CulinaryCodeDbContext _ctx;

    public GroceryRepository(CulinaryCodeDbContext ctx)
    {
        _ctx = ctx;
    }

    public GroceryList ReadGroceryListById(Guid id)
    {
        GroceryList? groceryList = _ctx.GroceryLists
            .Include(gl => gl.Ingredients)
            .ThenInclude(i => i.Ingredient)
            .Include(gl => gl.Items)
            .ThenInclude(i => i.GroceryItem)
            .Include(gl => gl.Account)
            .FirstOrDefault(gl => gl.GroceryListId == id);
        if (groceryList == null)
        {
            throw new GroceryListNotFoundException("Grocery list not found!");
        }
        
        return groceryList;
    }

    public GroceryList ReadGroceryListByAccountId(Guid accountId)
    {
        var groceryList = _ctx.GroceryLists
            .Include(gl => gl.Ingredients)
                .ThenInclude(i => i.Ingredient)
            .Include(gl => gl.Items)
                .ThenInclude(i => i.GroceryItem)
            .Include(gl => gl.Account)
            .FirstOrDefault(gl => gl.Account.AccountId == accountId);
        
        if (groceryList == null)
        {
            throw new GroceryListNotFoundException("No grocery list found for this account.");
        }

        return groceryList;
    }

    public void CreateGroceryList(GroceryList groceryList)
    {
        _ctx.GroceryLists.Add(groceryList);
        _ctx.SaveChanges();
    }

    public void UpdateGroceryList(GroceryList groceryList)
    {
        _ctx.GroceryLists.Update(groceryList);
        _ctx.SaveChanges();
    }
    
    public void AddGroceryListItem(GroceryList groceryList, ItemQuantity newItem)
    {
        if (groceryList == null || newItem == null)
        {
            throw new GroceryListNotFoundException("Grocery list or new item cannot be null.");
        }
        
        groceryList.Items.Append(newItem); 

        _ctx.ItemQuantities.Add(newItem);
        _ctx.GroceryItems.Add(newItem.GroceryItem);

        _ctx.SaveChanges();
    }
    
    public async Task DeleteItemFromGroceryList(Guid groceryListId, Guid itemId)
    {
        var groceryList = await _ctx.GroceryLists
            .Include(gl => gl.Items) 
            .ThenInclude(i => i.GroceryItem)
            .Include(gl => gl.Ingredients)
            .ThenInclude(i => i.Ingredient)
            .FirstOrDefaultAsync(gl => gl.GroceryListId == groceryListId);

        if (groceryList == null)
        {
            throw new GroceryListNotFoundException("Grocery list not found.");
        }
        
        var itemToDelete = groceryList.Items?.FirstOrDefault(i => i.ItemQuantityId == itemId); 

        if (itemToDelete != null)
        {
            GroceryItem? item = itemToDelete.GroceryItem;
            _ctx.ItemQuantities.Remove(itemToDelete);
            if (item != null) _ctx.GroceryItems.Remove(item);
        }
        else
        {
            var ingredientToDelete = groceryList.Ingredients?.FirstOrDefault(i => i.IngredientQuantityId == itemId);
            if (ingredientToDelete != null)
            {
                _ctx.IngredientQuantities.Remove(ingredientToDelete);
            }
            else
            {
                throw new GroceryListNotFoundException("Item not found.");
            }
        }
        await _ctx.SaveChangesAsync();
    }
}