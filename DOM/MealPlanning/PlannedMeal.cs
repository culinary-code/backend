using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    public MealPlanner? NextWeekMealPlanner { get; set; }
    public MealPlanner? HistoryMealPlanner { get; set; }
}