using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DOM.Recipes;
using DOM.Recipes.Ingredients;

namespace DOM.MealPlanning;

public class PlannedMeal
{
    [Key] public int PlannedMealId { get; set; }
    public int AmountOfPeople { get; set; }
    public Dictionary<Ingredient, int> Ingredients { get; set; } = new();
    public Recipe? Recipe { get; set; }
    
    // navigation properties
    private MealPlanner? NextWeekMealPlanner { get; set; }
    private MealPlanner? HistoryMealPlanner { get; set; }
}