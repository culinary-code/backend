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
}