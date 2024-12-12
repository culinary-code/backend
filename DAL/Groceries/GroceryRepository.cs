using DAL.EF;
using DOM.MealPlanning;
using DOM.Recipes.Ingredients;
using DOM.Results;
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

    // used to return a dto, doesn't need to be tracked
    public async Task<Result<GroceryList>> ReadGroceryListByIdNoTracking(Guid id)
    {
        GroceryList? groceryList = await _ctx.GroceryLists
            .Include(gl => gl.Ingredients)
            .ThenInclude(i => i.Ingredient)
            .Include(gl => gl.Items)
            .ThenInclude(i => i.GroceryItem)
            .Include(gl => gl.Account)
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync(gl => gl.GroceryListId == id);
        if (groceryList == null)
        {
            return Result<GroceryList>.Failure($"No groceryList found with id {id}", ResultFailureType.NotFound);
        }
        
        return Result<GroceryList>.Success(groceryList);
    }

    // used to update itemQuantity, needs to be tracked
    public async Task<Result<ItemQuantity>> ReadItemQuantityById(Guid id)
    {
        ItemQuantity? itemQuantity = await _ctx.ItemQuantities.FindAsync(id);
        if (itemQuantity == null)
        {
            return Result<ItemQuantity>.Failure($"No item quantity found with id {id}", ResultFailureType.NotFound);
        }
        return Result<ItemQuantity>.Success(itemQuantity);
    }

    // used to update grocerylist, needs to be tracked
    public async Task<Result<GroceryList>> ReadGroceryListByAccountId(Guid accountId)
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
            return Result<GroceryList>.Failure($"No grocery list found for account with id {accountId}", ResultFailureType.NotFound);
        }

        return Result<GroceryList>.Success(groceryList);
    }

    public async Task<Result<GroceryList>> ReadGroceryListByGroupId(Guid groupId)
    {
        var groceryList = await _ctx.GroceryLists
            .Include(gl => gl.Ingredients)
            .ThenInclude(i => i.Ingredient)
            .Include(gl => gl.Items)
            .ThenInclude(i => i.GroceryItem)
            .Include(gl => gl.Account)
            .FirstOrDefaultAsync(gl => gl.Group.GroupId == groupId);

        if (groceryList == null)
        {
            return Result<GroceryList>.Failure($"No grocery list found for group with id {groupId}", ResultFailureType.NotFound);
        }
        
        return Result<GroceryList>.Success(groceryList);
    }

    // used to update groceryList, needs to be tracked
    public async Task<Result<GroceryItem>> ReadGroceryItemByNameAndMeasurement(string name, MeasurementType measurementType)
    {
        var groceryItem = await _ctx.GroceryItems.FirstOrDefaultAsync(gi => gi.GroceryItemName == name && gi.Measurement == measurementType);
        if (groceryItem == null)
        {
            return Result<GroceryItem>.Failure($"No grocery item found with name: {name}", ResultFailureType.NotFound);
        }
        return Result<GroceryItem>.Success(groceryItem);
    }

    public async Task<Result<Unit>> CreateGroceryList(GroceryList groceryList)
    {
        await _ctx.GroceryLists.AddAsync(groceryList);
        await _ctx.SaveChangesAsync();
        return Result<Unit>.Success(new Unit());
    }

    public async Task<Result<Unit>> UpdateGroceryList(GroceryList groceryList)
    {
        _ctx.GroceryLists.Update(groceryList);
        await _ctx.SaveChangesAsync();
        return Result<Unit>.Success(new Unit());
    }
    
    public async Task<Result<Unit>> AddGroceryListItem(GroceryList groceryList, ItemQuantity newItem)
    {
        groceryList.Items.Add(newItem); 

        await _ctx.ItemQuantities.AddAsync(newItem);
        await _ctx.GroceryItems.AddAsync(newItem.GroceryItem);

        await _ctx.SaveChangesAsync();
        
        return Result<Unit>.Success(new Unit());
    }

    public async Task<Result<Unit>> DeleteItemQuantity(Guid userId, Guid itemId)
    {
        var itemQuantity = await _ctx.ItemQuantities
                .Include(i => i.GroceryList)
                .ThenInclude(i => i.Account)
                .FirstOrDefaultAsync(i => i.ItemQuantityId == itemId);
        if (itemQuantity == null)
        {
            return Result<Unit>.Failure($"No itemquantity found with id: {itemId}", ResultFailureType.NotFound);
        }
        
        if (itemQuantity.GroceryList!.Account!.AccountId == userId)
        {
            _ctx.ItemQuantities.Remove(itemQuantity);
            await _ctx.SaveChangesAsync();
            return Result<Unit>.Success(new Unit());
        }
        return Result<Unit>.Failure($"No itemquantity does not belong to user with id: {userId}", ResultFailureType.Error);

    }
}