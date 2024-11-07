using System;
using System.ComponentModel.DataAnnotations;
using DOM.MealPlanning;

namespace DOM.Recipes.Ingredients;

public class IngredientQuantity
{
    [Key]
    public Guid IngredientQuantityId { get; set; }
    public float Quantity { get; set; }
    public Ingredient? Ingredient { get; set; } 
    
    // navigation properties
    public Recipe? Recipe { get; set; }
    public GroceryList? GroceryList { get; set; }
    public PlannedMeal? PlannedMeal { get; set; }
}