using DAL.EF;
using DOM.Exceptions;
using DOM.MealPlanning;
using DOM.Recipes.Ingredients;
using Microsoft.EntityFrameworkCore;

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
            .ThenInclude(i => i. GroceryItem)
            .Include(gl => gl.Account)
            .FirstOrDefault(gl => gl.GroceryListId == id);
        if (groceryList == null)
        {
            throw new GroceryListNotFoundException();
        }

        var groupedIngredients = groceryList.Ingredients
            .Where(i => i != null)
            .GroupBy(i => i.Ingredient.IngredientId)
            .Select(group => new
            {
                IngredientId = group.Key,
                IngredientName = group.Key,
                Quantity = group.Sum(i => i.Quantity),
                MeasurementType = group.First().Ingredient.Measurement,
            }).ToList();

        var groupedItems = groceryList.Items
            .Where(i => i != null)
            .GroupBy(i => i.GroceryItem.GroceryItemId)
            .Select(group => new
            {
                GroceryItemId = group.Key,
                GroceryName = group.Key,
                Quantity = group.Sum(i => i.Quantity),
            }).ToList();
        
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

        return groceryList;
    }

    public void CreateNewGroceryList(GroceryList groceryList)
    {
        _ctx.GroceryLists.Add(groceryList);
        _ctx.SaveChanges();
    }

    public void CreateGroceryList(GroceryList groceryList)
    {
        var savedGroceryList = _ctx.GroceryLists
            .Include(gl => gl.Ingredients)
            .ThenInclude(iq => iq.Ingredient)
            .FirstOrDefault(gl => gl.GroceryListId == groceryList.GroceryListId);
        
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

        _ctx.Add(newItem);
        _ctx.Add(newItem.GroceryItem);

        _ctx.SaveChanges();
    }
}