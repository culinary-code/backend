using System;
using System.ComponentModel.DataAnnotations;
using DOM.MealPlanning;

namespace DOM.Recipes.Ingredients;

public class ItemQuantity
{
    public Guid ItemQuantityId { get; set; }
    public float Quantity { get; set; }
    public GroceryItem? GroceryItem { get; set; } 
    
    // navigation properties
    public GroceryList? GroceryList { get; set; }
    
    // Foreign keys
    public Guid? GroceryListId { get; set; }
    public Guid? GroceryItemId { get; set; }
}