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

    public ItemQuantity ReadItemQuantityById(Guid id)
    {
        ItemQuantity? itemQuantity = _ctx.ItemQuantities.Find(id);
        if (itemQuantity == null)
        {
            throw new ItemQuantityNotFoundException($"No itemQuantity found with id {id}");
        }
        return itemQuantity;
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

    public GroceryItem? ReadPossibleGroceryItemByNameAndMeasurement(string name, MeasurementType measurementType)
    {
        return _ctx.GroceryItems.FirstOrDefault(gi => gi.GroceryItemName == name && gi.Measurement == measurementType);
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

    public async Task DeleteItemQuantity(Guid userId, Guid itemId)
    {
        var itemQuantity = _ctx.ItemQuantities
                .Include(i => i.GroceryList)
                .ThenInclude(i => i.Account)
                .First(i => i.ItemQuantityId == itemId);
        if (itemQuantity.GroceryList!.Account!.AccountId == userId)
        {
            _ctx.ItemQuantities.Remove(itemQuantity);
            await _ctx.SaveChangesAsync();
        }
    }
}