using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using DOM.MealPlanning;

namespace DOM.Recipes.Ingredients;

public class IngredientQuantity
{
    [Key]
    public Guid IngredientQuantityId { get; set; }
    public int Quantity { get; set; }
    public Ingredient? Ingredient { get; set; } 
    
    // navigation properties
    public Recipe? Recipe { get; set; }
    public GroceryList? GroceryList { get; set; }
    public PlannedMeal? PlannedMeal { get; set; }
}