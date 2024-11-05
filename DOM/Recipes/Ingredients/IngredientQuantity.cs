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
    [JsonIgnore]
    public Recipe? Recipe { get; set; }
    [JsonIgnore]
    public GroceryList? GroceryList { get; set; }
    [JsonIgnore]
    public PlannedMeal? PlannedMeal { get; set; }
}