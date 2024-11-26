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
    ILogger<GroceryRepository> _logger;

    public GroceryRepository(CulinaryCodeDbContext ctx, ILogger<GroceryRepository> logger)
    {
        _ctx = ctx;
        _logger = logger;
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

    // GroceryListRepository.cs

    public async Task DeleteItemFromGroceryList(Guid groceryListId, Guid itemId)
    {
        // Load grocery list with its related items
        var groceryList = await _ctx.GroceryLists
            .Include(gl => gl.Items) // Eagerly load the items
            .ThenInclude(i => i.GroceryItem) // If needed, load related items
            .FirstOrDefaultAsync(gl => gl.GroceryListId == groceryListId);

        if (groceryList == null)
        {
            _logger.LogWarning("Grocery list {GroceryListId} not found.", groceryListId);
            throw new GroceryListNotFoundException("Grocery list not found.");
        }

        // Log the number of items in the grocery list
        _logger.LogInformation("Found {ItemCount} items in grocery list {GroceryListId}.",
            groceryList.Items?.Count() ?? 0, groceryListId);

        // Find the item to delete
        var itemToDelete =
            groceryList.Items?.FirstOrDefault(i => i.ItemQuantityId == itemId); // Make sure 'Id' is correct
        var ingredientToDelete = groceryList.Ingredients?.FirstOrDefault(i => i.IngredientQuantityId == itemId);

        if (itemToDelete == null && ingredientToDelete == null)
        {
            _logger.LogWarning("Item with ID {ItemId} not found in grocery list {GroceryListId}.", itemId,
                groceryListId);
            throw new GroceryListNotFoundException("Item not found.");
        }
        else if (ingredientToDelete != null)
        {
            _ctx.IngredientQuantities.Remove(ingredientToDelete);
        }
        else

            // Remove the item from the list
            _ctx.ItemQuantities.Remove(itemToDelete);

        await _ctx.SaveChangesAsync();

        _logger.LogInformation("Item {ItemId} successfully removed from grocery list {GroceryListId}.", itemId,
            groceryListId);
    }
}