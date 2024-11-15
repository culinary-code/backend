using System;
using System.ComponentModel.DataAnnotations;
using DOM.MealPlanning;

namespace DOM.Recipes.Ingredients;

public class ItemQuantity
{
    [Key]
    public Guid IngredientQuantityId { get; set; }
    public float Quantity { get; set; }
    public GroceryItem? GroceryItem { get; set; } 
    
    // navigation properties
    public GroceryList? GroceryList { get; set; }
}