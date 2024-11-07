using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DOM.Accounts;
using DOM.Recipes.Ingredients;

namespace DOM.MealPlanning;

public class GroceryList
{
    [Key] public Guid GroceryListId { get; set; }
    public IEnumerable<ItemQuantity> Items { get; set; } = new List<ItemQuantity>();
    public IEnumerable<IngredientQuantity> Ingredients { get; set; } = new List<IngredientQuantity>();
    
    // navigation properties
    public Account? Account { get; set; } 
}