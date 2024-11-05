using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using DOM.Recipes;
using DOM.Recipes.Ingredients;

namespace DOM.MealPlanning;

public class PlannedMeal
{
    [Key] public Guid PlannedMealId { get; set; }
    public int AmountOfPeople { get; set; }
    public IEnumerable<IngredientQuantity> Ingredients { get; set; } = new List<IngredientQuantity>();
    public Recipe? Recipe { get; set; }
    
    // navigation properties
    [JsonIgnore]
    public MealPlanner? NextWeekMealPlanner { get; set; }
    [JsonIgnore]
    public MealPlanner? HistoryMealPlanner { get; set; }
}