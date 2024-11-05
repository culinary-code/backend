using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using DOM.MealPlanning;

namespace DOM.Recipes.Ingredients;

public class ItemQuantity
{
    [Key]
    public Guid IngredientQuantityId { get; set; }
    public int Quantity { get; set; }
    public Ingredient? Ingredient { get; set; } 
    
    // navigation properties
    public GroceryList? GroceryList { get; set; }
}