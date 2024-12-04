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

    public async Task<GroceryList> ReadGroceryListById(Guid id)
    {
        GroceryList? groceryList = await _ctx.GroceryLists
            .Include(gl => gl.Ingredients)
            .ThenInclude(i => i.Ingredient)
            .Include(gl => gl.Items)
            .ThenInclude(i => i.GroceryItem)
            .Include(gl => gl.Account)
            .FirstOrDefaultAsync(gl => gl.GroceryListId == id);
        if (groceryList == null)
        {
            throw new GroceryListNotFoundException("Grocery list not found!");
        }
        
        return groceryList;
    }

    public async Task<ItemQuantity> ReadItemQuantityById(Guid id)
    {
        ItemQuantity? itemQuantity = await _ctx.ItemQuantities.FindAsync(id);
        if (itemQuantity == null)
        {
            throw new ItemQuantityNotFoundException($"No itemQuantity found with id {id}");
        }
        return itemQuantity;
    }

    public async Task<GroceryList> ReadGroceryListByAccountId(Guid accountId)
    {
        var groceryList = await _ctx.GroceryLists
            .Include(gl => gl.Ingredients)
                .ThenInclude(i => i.Ingredient)
            .Include(gl => gl.Items)
                .ThenInclude(i => i.GroceryItem)
            .Include(gl => gl.Account)
            .FirstOrDefaultAsync(gl => gl.Account.AccountId == accountId);
        
        if (groceryList == null)
        {
            throw new GroceryListNotFoundException("No grocery list found for this account.");
        }

        return groceryList;
    }

    public async Task<GroceryItem?> ReadPossibleGroceryItemByNameAndMeasurement(string name, MeasurementType measurementType)
    {
        return await _ctx.GroceryItems.FirstOrDefaultAsync(gi => gi.GroceryItemName == name && gi.Measurement == measurementType);
    }

    public async Task CreateGroceryList(GroceryList groceryList)
    {
        await _ctx.GroceryLists.AddAsync(groceryList);
        await _ctx.SaveChangesAsync();
    }

    public async Task UpdateGroceryList(GroceryList groceryList)
    {
        _ctx.GroceryLists.Update(groceryList);
        await _ctx.SaveChangesAsync();
    }
    
    public async Task AddGroceryListItem(GroceryList groceryList, ItemQuantity newItem)
    {
        if (groceryList == null || newItem == null)
        {
            throw new GroceryListNotFoundException("Grocery list or new item cannot be null.");
        }
        
        groceryList.Items.Add(newItem); 

        await _ctx.ItemQuantities.AddAsync(newItem);
        await _ctx.GroceryItems.AddAsync(newItem.GroceryItem);

        await _ctx.SaveChangesAsync();
    }

    public async Task DeleteItemQuantity(Guid userId, Guid itemId)
    {
        var itemQuantity = await _ctx.ItemQuantities
                .Include(i => i.GroceryList)
                .ThenInclude(i => i.Account)
                .FirstAsync(i => i.ItemQuantityId == itemId);
        if (itemQuantity.GroceryList!.Account!.AccountId == userId)
        {
            _ctx.ItemQuantities.Remove(itemQuantity);
            await _ctx.SaveChangesAsync();
        }
    }
}