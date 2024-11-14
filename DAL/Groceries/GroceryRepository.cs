using DAL.EF;
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
            .Include(gl => gl.Items)
            .Include(gl => gl.Account)
            .FirstOrDefault(gl => gl.GroceryListId == id);
        if (groceryList == null)
        {
            throw new KeyNotFoundException();
        }
        return groceryList;
    }

    public void CreateGroceryList(GroceryList groceryList)
    {
        var savedGroceryList = _ctx.GroceryLists
            .Include(gl => gl.Ingredients)
            .ThenInclude(iq => iq.Ingredient)
            .FirstOrDefault(gl => gl.GroceryListId == groceryList.GroceryListId);

        Console.WriteLine($"Saved Ingredients Count: {savedGroceryList?.Ingredients.Count() ?? 0}");
        foreach (var ingredient in savedGroceryList?.Ingredients ?? new List<IngredientQuantity>())
        {
            Console.WriteLine($"Saved Ingredient: {ingredient.Ingredient?.IngredientName}, Quantity: {ingredient.Quantity}");
        }
    }

    public void UpdateGroceryList(GroceryList groceryList)
    { 
        _ctx.GroceryLists.Update(groceryList);
        _ctx.SaveChanges();
    }
}