﻿using System.Collections.Generic;
using System.Text.Json.Serialization;
using DOM.Accounts;
using DOM.Recipes.Ingredients;

namespace DOM.MealPlanning;

public class GroceryList
{
    public int GroceryListId { get; set; }
    public IEnumerable<ItemQuantity> Items { get; set; } = new List<ItemQuantity>();
    public IEnumerable<IngredientQuantity> Ingredients { get; set; } = new List<IngredientQuantity>();
    
    // navigation properties
    [JsonIgnore]
    public Account? Account { get; set; } 
}