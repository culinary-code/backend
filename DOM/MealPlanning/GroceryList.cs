using System.Collections.Generic;
using DOM.Accounts;
using DOM.Recipes.Ingredients;

namespace DOM.MealPlanning;

public class GroceryList
{
    public int GroceryListId { get; set; }
    public Dictionary<string, int> Items { get; set; } = new();
    public Dictionary<Ingredient, int> Ingredients { get; set; } = new();
    
    // navigation properties
    private Account? Accounts { get; set; } 
}