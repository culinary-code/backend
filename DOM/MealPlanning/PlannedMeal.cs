using System.ComponentModel.DataAnnotations;
using DOM.Recipes;
using DOM.Recipes.Ingredients;

namespace DOM.MealPlanning;

public class PlannedMeal
{
    public Guid PlannedMealId { get; set; }
    public int AmountOfPeople { get; set; }
    public ICollection<IngredientQuantity> Ingredients { get; set; } = new List<IngredientQuantity>();
    public Recipe? Recipe { get; set; }
    
    public DateTime PlannedDate { get; set; }
    
    // navigation properties
    public MealPlanner? NextWeekMealPlanner { get; set; }
    public MealPlanner? HistoryMealPlanner { get; set; }
    
    // Foreign keys
    public Guid? RecipeId { get; set; }
    public Guid? NextWeekMealPlannerId { get; set; }
    public Guid? HistoryMealPlannerId { get; set; }
}
